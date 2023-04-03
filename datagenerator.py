import csv
from pathlib import Path
from sys import __stdout__

import cv2
import mediapipe as mp

from settings import *

mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_face_mesh = mp.solutions.face_mesh
mp_face_mesh_connections = mp.solutions.face_mesh_connections

BLENDSHAPE_I = []
WEIGHTS = []
IMAGE_FILES = []

csv_file = open(BLENDSHAPE_FILE, "r")
reader = csv.reader(csv_file, skipinitialspace=True, delimiter=",")
## skip header
next(reader, None)
for line in reader:
    if not Path(line[2]).exists():
        print(line[2])
        print("File not found")
        continue
    BLENDSHAPE_I.append(line[0])
    WEIGHTS.append(line[1])
    IMAGE_FILES.append(line[2])
csv_file.close()

train_file = open(TRAIN_FILE, "w")
writer = csv.writer(train_file, delimiter=",", quotechar='"')
writer.writerow(HEADERS)

# write a model selection


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
        arr = [BLENDSHAPE_I[idx], WEIGHTS[idx]]
        for face_landmarks in results.multi_face_landmarks:
            for a in face_landmarks.landmark:
                arr.append(a.x)
                arr.append(a.y)
                arr.append(a.z)

        # 显示带有面部标记点的图像
        cv2.imshow("Face Mesh", image)
        cv2.waitKey(0)
        cv2.destroyAllWindows()
        print(len(arr))
        print((len(arr) - 2) / 3)
        break
        # writer.writerow(arr)

train_file.close()
