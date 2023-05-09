BLENDSHAPE_FILE = "./blendshapes.csv"
TRAIN_FILE = "/Users/lokeyli/Documents/Unity/Unity-Web-socket/train.csv"
N_LANDMARKS = 478
N_BLENDSHAPE = 48
FIRST_BLENDSHAPE_IDX = 67
HEADERS = ["blendshape_i", "weight"]
HEADERS.extend([f"landmark_{i}" for i in range(N_LANDMARKS)])

if __name__ == "__main__":
    print(HEADERS)
    print(len(HEADERS))
