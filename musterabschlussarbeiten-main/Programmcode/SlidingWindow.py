 
def display_selected_graphs(pointLists, filenames, selected_indices):
    selected_pointLists = []
    for index in selected_indices:
        selected_pointLists.append(pointLists[index])


    min_length = min(len(points_list) for points_list in selected_pointLists)
    selected_pointLists_clipped = [points_list[:min_length] for points_list in selected_pointLists]

    fig, ax = plt.subplots()

    for i, points_list in zip(selected_indices, selected_pointLists_clipped):
        x = np.arange(1, len(points_list) + 1)
        ax.plot(x, points_list, label=f'{filenames[i]}')

    ax.set_xlabel('Episode')
    ax.set_ylabel('Average Points per Game')
    ax.set_title(f'Average Points per Game over {amount_of_games} Games')
    ax.legend()

    plt.show()
