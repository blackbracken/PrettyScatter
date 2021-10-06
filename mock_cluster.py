import random as rnd
import math

WIDTH = 80
MAX_X = WIDTH / 2
MIN_X = -WIDTH / 2

HEIGHT = 60
MAX_Y = HEIGHT / 2
MIN_Y = -HEIGHT / 2

CLUSTER_SIZE = 3.0

PLOT_NUM = 2000
CLUSTER_NUM = 5

if PLOT_NUM % CLUSTER_NUM != 0:
    raise RuntimeError()

with open("./mock_cluster.dat", mode="w") as f:
    for cluster in range(CLUSTER_NUM):
        center_x = rnd.randint(int(MIN_X / 2), int(MAX_X / 2))
        center_y = rnd.randint(int(MIN_Y / 2), int(MAX_Y / 2))

        for i in range(int(PLOT_NUM / CLUSTER_NUM)):
            x = center_x + math.cos(math.radians(360.0 * rnd.random())) * (CLUSTER_SIZE * rnd.random())
            if (rnd.random() < 0.34):
                 x += (x if rnd.random() < 0.5 else (-2 * x)) * rnd.random()

            y = center_y + math.sin(math.radians(360.0 * rnd.random())) * (CLUSTER_SIZE * rnd.random())
            if (rnd.random() < 0.34):
                 y += (y if rnd.random() < 0.5 else (-2 * y)) * rnd.random()

            f.write(f"{x}, {y}, {cluster}\n")