from pickle import load
import pandas as pd
import sklearn as sk
from settings import *
import cv2
import mediapipe as mp

mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_face_mesh = mp.solutions.face_mesh

IMAGE_FILES = ["/Users/lokeyli/Documents/Unity/Unity-Web-socket/test-img-01.jpg"]

if __name__ == "__main__":
    # Load the saved model from disk
    blendshape_i_lst = (
        pd.read_csv(
            TRAIN_FILE,
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

    models = []
    for blendshape_i in blendshape_i_lst:
        with open(f"fm2bs_model_{blendshape_i}.pkl", "rb") as f:
            model = load(f)
            models.append(model)

    # predict the landmarks to blendshape
    arr = []
    drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)
    with mp_face_mesh.FaceMesh(
        static_image_mode=True,
        max_num_faces=1,
        refine_landmarks=True,
        min_detection_confidence=0.5,
    ) as face_mesh:
        for idx, file in enumerate(IMAGE_FILES):
            image = cv2.imread(file)
            # Convert the BGR image to RGB before processing.
            results = face_mesh.process(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))

            # Print and draw face mesh landmarks on the image.
            if not results.multi_face_landmarks:
                continue
            annotated_image = image.copy()
            for face_landmarks in results.multi_face_landmarks:
                for a in face_landmarks.landmark:
                    arr.append(a.x)
                    arr.append(a.y)
                    arr.append(a.z)
            predict_df = pd.DataFrame([arr], columns=HEADERS[2:])
            for idx, model in enumerate(models):
                # selectors[idx].transform(predict_df)
                print(model.predict(selectors[idx].transform(predict_df)))
