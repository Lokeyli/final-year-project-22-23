BLENDSHAPE_FILE = "blendshapes.csv"
TRAIN_FILE = "train.csv"
N_LANDMARKS = 478
N_BLENDSHAPE = 48
HEADERS = ["blendshape_i", "blendshape_name", "weight"]
HEADERS.extend([f"landmark_{i}" for i in range(N_LANDMARKS)])

if __name__ == "__main__":
    print(HEADERS)
    print(len(HEADERS))
