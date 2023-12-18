using System;
using TaskManager.Models;
using System.Collections.Generic;

namespace TaskManager.Services
{
  public class TaskItemList
  {

    private List<TaskItem> tasks;
    private const int MaxTasks = 6; 

    public TaskItemList()
    {
        tasks = new List<TaskItem>(MaxTasks);
    }

    public bool AddTask(string taskValue)
    {
        if (tasks.Count >= MaxTasks)
        {
            return false;
        }
        var newTask = new TaskItem(taskValue);
        tasks.Add(newTask);
        newTask.TaskIndex = tasks.Count;
        return true;
    }

    public IReadOnlyList<TaskItem> GetAllTasks()
    {
        return tasks.AsReadOnly();
    }

    public void DeleteAllItems()
    {
      tasks.Clear();
    }

    public void DeleteItemByIndex(int index)
    {
        tasks.RemoveAt(index);
    }
        public TaskItem GetTask(int index)
    {
        if (index >= 0 && index < tasks.Count)
        {
            return tasks[index];
        }

        throw new ArgumentOutOfRangeException(nameof(index), "The index is out of range of the tasks collection.");
    }

  }
    
}
