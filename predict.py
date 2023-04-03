import asyncio
import json
import socket
import threading
from pickle import load
from queue import Queue

import cv2
import mediapipe as mp
import numpy as np
import pandas as pd
import sklearn as sk
from pandas import DataFrame

from settings import *

mp_drawing = mp.solutions.drawing_utils  # type: ignore
mp_drawing_styles = mp.solutions.drawing_styles  # type: ignore
mp_face_mesh = mp.solutions.face_mesh  # type: ignore
mp_face_mesh_connections = mp.solutions.face_mesh_connections  # type: ignore

from typing import Any, Callable, Dict, List, Optional, Set, Tuple


def distance_for_prediction(input_df: DataFrame) -> DataFrame:
    """Transform the input DataFrame to the format for prediction.

    478 (x, y, z) coordinates -> distances.
    Args:
        input_df (DataFrame): The input DataFrame generated base on MediaPIpe FaceMesh.

    Returns:
        DataFrame: The output DataFrame for prediction.
    """

    vertices_sets: Dict[str, Set[Tuple[int, int]]] = {
        "FACEMESH_FACE_OVAL": mp_face_mesh_connections.FACEMESH_FACE_OVAL,
        "FACEMESH_LIPS": mp_face_mesh_connections.FACEMESH_LIPS,
        "FACEMESH_LEFT_EYE": mp_face_mesh_connections.FACEMESH_LEFT_EYE,
        "FACEMESH_LEFT_IRIS": mp_face_mesh_connections.FACEMESH_LEFT_IRIS,
        "FACEMESH_LEFT_EYEBROW": mp_face_mesh_connections.FACEMESH_LEFT_EYEBROW,
        "FACEMESH_RIGHT_EYE": mp_face_mesh_connections.FACEMESH_RIGHT_EYE,
        "FACEMESH_RIGHT_EYEBROW": mp_face_mesh_connections.FACEMESH_RIGHT_EYEBROW,
        "FACEMESH_RIGHT_IRIS": mp_face_mesh_connections.FACEMESH_RIGHT_IRIS,
    }

    # define the column names
    distance_train_columns = list()
    for name, vertices_set in vertices_sets.items():
        for idx, vertices in enumerate(vertices_set):
            column = f"{name}_distance_{idx}"
            distance_train_columns.append(column)

    distance_train_data = DataFrame(columns=distance_train_columns, dtype=np.float64)

    for i, row in input_df.iterrows():
        new_row = list()
        for name, vertices_set in vertices_sets.items():
            for idx, vertices in enumerate(vertices_set):
                new_row.append(np.linalg.norm(row[vertices[0]] - row[vertices[1]]))
        distance_train_data.loc[0] = new_row  # type: ignore

    return distance_train_data


def websocket_client():
    server_address = ("localhost", 8080)
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client.connect(server_address)
    while True:
        global blendshape_weights
        global terminate_event
        if terminate_event.is_set():
            break
        if blendshape_weights.empty():
            continue
        json_weights = json.dumps(blendshape_weights.get())
        print(json_weights)
        client.sendall(json_weights.encode("utf-8"))
        # client.close()


def load_models() -> Tuple[List[Any], List[Any]]:
    """Load the saved feature selection and prediction models.

    Returns:
        Tuple[List[Any], List[Any]]: The tuple(list of Selectors, list of Predictors)
    """
    blendshape_i_lst = (
        pd.read_csv(
            TRAIN_FILE,  # type: ignore
            header=0,
            names=HEADERS,
            delimiter=",",
            index_col=False,
            usecols=[0],
        )["blendshape_i"]
        .drop_duplicates()
        .to_list()
    )

    selectors = []
    for blendshape_i in blendshape_i_lst:
        with open(f"fm2bs_selector_{blendshape_i}.pkl", "rb") as f:
            selector = load(f)
            selectors.append(selector)

    predictors = []
    for blendshape_i in blendshape_i_lst:
        with open(f"fm2bs_model_{blendshape_i}.pkl", "rb") as f:
            model = load(f)
            predictors.append(model)

    return selectors, predictors


def main_loop():
    cap = cv2.VideoCapture(0)
    with mp_face_mesh.FaceMesh(
        max_num_faces=1,
        refine_landmarks=True,
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5,
    ) as face_mesh:
        while cap.isOpened():
            success, image = cap.read()
            if not success:
                print("Ignoring empty camera frame.")
                # If loading a video, use 'break' instead of 'continue'.
                continue

            # To improve performance, optionally mark the image as not writeable to
            # pass by reference.
            image.flags.writeable = False
            image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            results = face_mesh.process(image)

            # Draw the face mesh annotations on the image.
            image.flags.writeable = True
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
            arr = []
            if results.multi_face_landmarks is None:
                continue
            for face_landmarks in results.multi_face_landmarks:
                for a in face_landmarks.landmark:
                    arr.append(np.array([a.x, a.y, a.z]))
            predict_df = pd.DataFrame([arr], columns=HEADERS[2:])
            blendshape_weight = []
            for idx, model in enumerate(predictors):
                blendshape_weight.append(
                    int(
                        model.predict(
                            selectors[idx].transform(
                                distance_for_prediction(predict_df)
                            )
                        )[0]
                    )
                )
            global blendshape_weights
            blendshape_weights.put(blendshape_weight)


IMAGE_FILES = ["/Users/lokeyli/Documents/Unity/Unity-Web-socket/index83-weight$100.png"]

if __name__ == "__main__":
    blendshape_weights = Queue()
    selectors, predictors = load_models()
    print("Models loaded.")

    # Terminate on KeyboardInterrupt
    terminate_event = threading.Event()

    # Start the websocket thread
    socket_client_thread = threading.Thread(target=websocket_client)
    socket_client_thread.start()
    print("Websocket thread started.")

    try:
        main_loop()
    except KeyboardInterrupt:
        terminate_event.set()
        socket_client_thread.join()
        print("Program terminated.")
        exit(0)
