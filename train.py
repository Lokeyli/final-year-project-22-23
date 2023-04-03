import csv
from ast import literal_eval
from pickle import dump

import numpy as np
import pandas as pd
import sklearn as sk
from numpy import array, ndim
from pandas import DataFrame
from sklearn.decomposition import TruncatedSVD
from sklearn.ensemble import RandomForestRegressor
from sklearn.model_selection import train_test_split

from settings import *


def method_selection() -> None:
    pass


def train() -> None:
    pass


if __name__ == "__main__":
    train_df = pd.read_csv(
        TRAIN_FILE, header=0, names=HEADERS, delimiter=",", index_col=False
    )
    blendshape_i_lst = train_df["blendshape_i"].drop_duplicates().to_list()
    for blendshape_i in blendshape_i_lst:
        sub_df = train_df[train_df["blendshape_i"] == blendshape_i]
        X = sub_df.filter(regex="landmark.*")
        Y = sub_df.filter(regex="weight").to_numpy().flatten()
        train_X, test_X, train_Y, test_Y = train_test_split(
            X, Y, test_size=0.2, random_state=42, shuffle=True
        )

        # breakpoint()

        selector = TruncatedSVD(n_components=20)
        selector.fit(train_X, train_Y)
        transformedX = selector.transform(train_X)

        model = RandomForestRegressor()
        model.fit(transformedX, train_Y)

        transformed_test_X = selector.transform(test_X)
        test_result = model.predict(transformed_test_X)
        print("transformed_test_X", transformed_test_X)
        print("test res", test_result)
        print("test Y", test_Y)
        print(model.score(transformed_test_X, test_Y))

        break
        # with open(f"fm2bs_selector_{blendshape_i}.pkl", "wb") as f:
        #     dump(pca, f)
        # with open(f"fm2bs_model_{blendshape_i}.pkl", "wb") as f:
        #     dump(model, f)

    # print(pca.transform(X))
    # print(train_df.head)
    # print(train_df["weight"])
    # print(pca.transform(newX))
    # map(literal_eval, train_df)
    # pca = TruncatedSVD(n_components=20)
    # pca.fit(train_df)
