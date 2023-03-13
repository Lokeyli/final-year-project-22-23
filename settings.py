BLENDSHAPE_FILE = "blendshapes.csv"
TRAIN_FILE = "train.csv"
N_LANDMARKS = 478
HEADERS = ["blendshape_i", "weight"]
HEADERS.extend(
    [f"landmark_{i}_{j}" for i in range(N_LANDMARKS) for j in ["x", "y", "z"]]
)

if __name__ == "__main__":
    print(HEADERS)
    print(len(HEADERS))
