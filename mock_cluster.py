import random as rnd

WIDTH = 60
MAX_X = WIDTH / 2
MIN_X = -WIDTH / 2

HEIGHT = 40
MAX_Y = HEIGHT / 2
MIN_Y = -HEIGHT / 2

PLOT_NUM = 20000
CLUSTER_NUM = 5

if PLOT_NUM % CLUSTER_NUM != 0:
    raise RuntimeError()

with open("./mock_cluster.dat", mode="w") as f:
    for cluster in range(CLUSTER_NUM):
        center_x = rnd.randint(int(MIN_X / 2), int(MAX_X / 2))
        center_y = rnd.randint(int(MIN_Y / 2), int(MAX_Y / 2))

        for i in range(int(PLOT_NUM / CLUSTER_NUM)):
            x = rnd.randint(int(MIN_X / 4), int(MAX_X / 4)) + center_x + rnd.random() / 2
            y = rnd.randint(int(MIN_Y / 4), int(MAX_Y / 4)) + center_y + rnd.random() / 2

            f.write(f"{x}, {y}, {cluster}\n")