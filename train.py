import sklearn as sk
from sklearn.decomposition import TruncatedSVD
from settings import *
import csv
from ast import literal_eval
import numpy as np
import pandas as pd
from numpy import ndim, array
from pandas import DataFrame
from pickle import dump


def pca():
    pass


def train():
    pass


if __name__ == "__main__":
    train_df = pd.read_csv(
        TRAIN_FILE, header=0, names=HEADERS, delimiter=",", index_col=False
    )
    blendshape_i_lst = train_df["blendshape_i"].drop_duplicates().to_list()
    for blendshape_i in blendshape_i_lst:
        pca = TruncatedSVD(n_components=20)
        sub_df = train_df[train_df["blendshape_i"] == blendshape_i]
        X = sub_df.filter(regex="landmark.*")
        Y = sub_df.filter(regex="weight").to_numpy().flatten()
        pca.fit(X)
        with open(f"fm2bs_selector_{blendshape_i}.pkl", "wb") as f:
            dump(pca, f)
        newX = pca.transform(X)
        model = sk.linear_model.LogisticRegression()
        model.fit(newX, Y)
        with open(f"fm2bs_model_{blendshape_i}.pkl", "wb") as f:
            dump(model, f)

    # print(pca.transform(X))
    # print(train_df.head)
    # print(train_df["weight"])
    # print(pca.transform(newX))
    # map(literal_eval, train_df)
    # pca = TruncatedSVD(n_components=20)
    # pca.fit(train_df)
