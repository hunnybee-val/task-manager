using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Interactivity;
using System.Diagnostics;
using System;
using System.Linq;
using TaskManager.Models;
using TaskManager.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

using Avalonia.Threading;
using System.Drawing.Printing;
using Avalonia.Styling;
using System.Runtime.InteropServices.JavaScript;
using Avalonia;


namespace TaskManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
    }

    //глобальные переменные
    TaskItemList tasksList = new TaskItemList();

    private Dictionary<TaskItem, (ProgressBar ProgressBar, TextBlock ProgressCounter)> taskToGuiMap =
    new Dictionary<TaskItem, (ProgressBar, TextBlock)>();

   
    //команды кнопок
    public async void AddTask(object source, RoutedEventArgs args)
    {
        var button = source as Button;
        if (tasksList.GetAllTasks().Count() < 5)
        {
            tasksList.AddTask(button.Tag.ToString());
            await CreateNewTask(tasksList.GetAllTasks().Last());
            DisplayLogMessage($"Была добавлена задача типа {button.Tag.ToString()}");
        }
        else
        {
            DisplayLogMessage("Добавлено максимальное количество задач.");
        }

    }

    public void StartAll(object source, RoutedEventArgs args)
    {
        var tasks = tasksList.GetAllTasks();
        foreach (var taskItemContainer in TaskPanel.Children)
        {
            taskItemContainer.Classes.Remove("pausedTask");
        }
        foreach (var taskItem in tasks)
        {


            if (taskToGuiMap.TryGetValue(taskItem, out var guiComponents) & !taskItem.IsGoing)
            {
                taskItem.IsGoing = true;
                taskItem.CancellationTokenSource = new CancellationTokenSource();
                _ = StartTask(guiComponents.ProgressBar, guiComponents.ProgressCounter, taskItem);
            }
        }
        DisplayLogMessage("Все задачи запущены");
        ButtonHandler();
    }

    public void ToggleTask(object source, RoutedEventArgs args)
    {
        var button = source as Button;

        if (button.Parent.Classes.Contains("isToggled"))
        {
            button.Parent.Classes.Remove("isToggled");
        }
        else
        {
            button.Parent.Classes.Add("isToggled");
        }

    }

    public void PauseAll(object source, RoutedEventArgs args)
    {
        var tasks = tasksList.GetAllTasks();
        foreach (var taskItemContainer in TaskPanel.Children)
        {
            taskItemContainer.Classes.Add("pausedTask");
        }
        foreach (var taskItem in tasks)
        {
            taskItem.IsGoing = false;

            taskItem.CancellationTokenSource?.Cancel();
        }

        DisplayLogMessage("Все задачи остановлены");
        ButtonHandler();
    }

    public void DeleteTask(object source, RoutedEventArgs args)
    {

        var button = source as Button;
        var parent = button.Parent as Grid;
        TaskPanel.Children.Remove(parent);
        tasksList.DeleteItemByIndex(Convert.ToInt32(button.Tag) - 1);
        DisplayLogMessage($"Была удалена задача №{button.Tag.ToString()}");
        ButtonHandler();
    }
    public void DeleteAll(object source, RoutedEventArgs args)
    {
        foreach (var taskItem in tasksList.GetAllTasks())
        {
            taskItem.IsGoing = false;

            taskItem.CancellationTokenSource?.Cancel();
        }
        TaskPanel.Children.Clear();
        tasksList.DeleteAllItems();
        DisplayLogMessage("Все задачи удалены");
        ButtonHandler();
    }

    public void TypeFilter(object source, RoutedEventArgs args)
    {
        var button = source as Button;
        if (button.Classes.Contains("hiddenTasks"))
        {
            TaskPanel.Classes.Remove($"HideObjects{button.Tag.ToString()}");
            button.Classes.Remove("hiddenTasks");
        }
        else
        {
            TaskPanel.Classes.Add($"HideObjects{button.Tag.ToString()}");
            button.Classes.Add("hiddenTasks");
        }
    }

    //отправка сообщений в лог
    private void DisplayLogMessage(string messageText)
    {
        var newMessage = new LogMessageItem(messageText);

        Grid logMessageContainer = new Grid();
        ColumnDefinition time = new ColumnDefinition()
        {
            Width = new GridLength(1, GridUnitType.Star)
        };
        ColumnDefinition message = new ColumnDefinition()
        {
            Width = new GridLength(3.3, GridUnitType.Star)
        };

        logMessageContainer.ColumnDefinitions.Add(time);
        logMessageContainer.ColumnDefinitions.Add(message);

        TextBlock messageTime = new TextBlock()
        {
            Text = $"{newMessage.TimeOfMessage}",
            
        };
        TextBlock messageContent = new TextBlock()
        {
            Text = $"{newMessage.LogMessage}",
            Opacity = 0.6
        };

        Grid.SetColumn(messageTime, 0);
        Grid.SetColumn(messageContent, 1);

        logMessageContainer.Children.Add(messageTime);
        logMessageContainer.Children.Add(messageContent);

        logMessageContainer.Classes.Add("logMessage");
        LogPanel.Children.Add(logMessageContainer);

        if (LogPanel.Children.Count > 3)
        {
            LogPanel.Children.RemoveAt(0);
        }
    }
    //переключатель кнопок
    private void ButtonHandler()
    {
        var allTasks = tasksList.GetAllTasks();
        bool anyTasksExist = allTasks.Count > 0;

        DeleteAllButton.IsEnabled = anyTasksExist;
        PauseAllButton.IsEnabled = anyTasksExist;
        StartAllButton.IsEnabled = anyTasksExist;

        if (anyTasksExist)
        {
            bool allTasksCompleteOrRunning = allTasks.All(t => t.IsGoing || t.ProgressValue == 100);
            bool allTasksCompleteOrNotRunning = allTasks.All(t => !t.IsGoing || t.ProgressValue == 100);

            PauseAllButton.IsEnabled = !allTasksCompleteOrNotRunning;

            StartAllButton.IsEnabled = !allTasksCompleteOrRunning;
        }
    }

    //создание новой задачи
    private async Task CreateNewTask(TaskItem newTask)
    {

        Grid grid = new Grid();
        grid.Classes.Add(newTask.TypeOfTask);
        grid.Classes.Add("taskContainer");
        grid.Classes.Add("isToggled");

        ColumnDefinition colDef1 = new ColumnDefinition()
        {
            Width = new GridLength(2.5, GridUnitType.Star)
        };
        ColumnDefinition colDef2 = new ColumnDefinition()
        {
            Width = new GridLength(0.7, GridUnitType.Star)
        };
        ColumnDefinition colDef3 = new ColumnDefinition()
        {
            Width = new GridLength(1, GridUnitType.Star)
        }; ;
        grid.ColumnDefinitions.Add(colDef1);
        grid.ColumnDefinitions.Add(colDef2);
        grid.ColumnDefinitions.Add(colDef3);

        ProgressBar task = new ProgressBar();

        Button toggleTask = new Button()
        {
            Height = 50,
            Opacity = 0
        };
        TextBlock progressCounter = new TextBlock();
        TextBlock taskIndex = new TextBlock()
        {

            Text = $"Задача №{newTask.TaskIndex}",
            Background = new SolidColorBrush(Color.Parse("#363636")),
            Height = 50,
            Width = 104

        };
        Button deleteTask = new Button()
        {
            Background = new SolidColorBrush(Color.Parse("#545454")),
            Height = 50,
            Content = "Удалить"
        };


        deleteTask.Tag = $"{newTask.TaskIndex}";
        deleteTask.Classes.Add("taskOptions");
        deleteTask.Click += DeleteTask;

        taskIndex.Classes.Add("taskOptions");
        taskIndex.Padding = new Avalonia.Thickness(0, 17, 0, 0);

        taskIndex.Classes.Add("toggleButton");
        toggleTask.Click += ToggleTask;

        Grid.SetColumn(toggleTask, 0);
        Grid.SetColumnSpan(toggleTask, 3);
        Grid.SetColumn(task, 0);
        Grid.SetColumn(progressCounter, 0);
        Grid.SetColumn(taskIndex, 1);
        Grid.SetColumn(deleteTask, 2);

        TaskPanel.Children.Add(grid);


        grid.Children.Add(task);
        grid.Children.Add(progressCounter);
        grid.Children.Add(toggleTask);
        grid.Children.Add(taskIndex);
        grid.Children.Add(deleteTask);


        task.Height = 50;
        task.Maximum = 100;

        progressCounter.Margin = new Avalonia.Thickness(10, 10, 10, 10);
        progressCounter.Text = $"Не активна";

        taskToGuiMap[newTask] = (task, progressCounter);

        if (newTask.IsGoing)
        {
            await StartTask(task, progressCounter, newTask);
        }
        else
        {
            progressCounter.Text = $"Не активна";
        }

        ButtonHandler();

    }

    //обработка прогресса задачи

    private async Task ProcessData(IProgress<TaskItem> progress, TaskItem newTask, CancellationToken cancellationToken, ProgressBar task)
    {
        for (int i = newTask.ProgressValue; i < 100; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            newTask.ProgressValue++;
            progress.Report(newTask);
            await Task.Delay(newTask.TimeRequired);
        }

    }


    public async Task StartTask(ProgressBar task, TextBlock progressCounter, TaskItem newTask)
    {
        var progress = new Progress<TaskItem>();
        progress.ProgressChanged += (o, report) =>
        {
            task.Value = report.ProgressValue;
            progressCounter.Text = $"{report.ProgressValue} / 100";
        };



        await ProcessData(progress, newTask, newTask.CancellationTokenSource.Token, task);

        if (!newTask.CancellationTokenSource.IsCancellationRequested)
        {
            if (newTask.IsSuccesful)
            {
                DisplayLogMessage($"Задача №{newTask.TaskIndex} завершилась успешно");
                task.Value = 100;
                progressCounter.Text = "100 / 100";
            }
            else
            {
                DisplayLogMessage($"Задача №{newTask.TaskIndex} завершилась с ошибкой");
                task.Value = 0;
                progressCounter.Text = "0 / 100";
                progressCounter.Parent.Classes.Add("errorTask");
            }
            ButtonHandler();
        }
    }

   

}

       
      
