import matplotlib.pyplot as plt
import numpy as np

def format_float(value):
    return "{:.3f}".format(value)

file_paths = [
    'LogPoints_trained.txt',
    'LogPoints_trained_with_more_turns_2.txt',
    'LogPoints_orange_field_untrained.txt',
    'LogPoints_orange_field_trained2.txt',
    'LogPoints_special_trained.txt',
    'LogPoints_untrained.txt',
    'miniField_trained.txt',
    'OnlyDice.txt'
    ]

filenames = [
    'trained',
    'more_turns',
    'orange_field_untrained',
    'orange_field_trained',
    'special_trained',
    'untrained',
    'minifield_training',
    'only_dice'
    ]


pointLists = []
for file_path in file_paths:
    points_list = []
    with open(file_path, 'r') as file:
        points = []
        amount_of_games = 2500
        for line in file:
            if line.strip():
                points.append(float(line.strip()))
                if len(points) > amount_of_games:
                    moving_average = sum(points[-amount_of_games:]) / amount_of_games
                    points_list.append(moving_average)
    pointLists.append(points_list)

min_length = min(len(points_list) for points_list in pointLists)
pointLists_clipped = [points_list[:min_length] for points_list in pointLists]
