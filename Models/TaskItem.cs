using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManager.Models
{
  public class TaskItem
  {
    private string? typeOfTask; 
    public string TypeOfTask
    {
      get => typeOfTask ?? throw new InvalidOperationException("Type of a new task is not set."); 
      set => typeOfTask = (value == "A" || value == "B") ? value : "A";
    }

    public bool IsSuccesful {get; set;}
    public bool IsGoing {get; set;}
    public int TimeRequired {get; set;}
    public int ProgressValue {get; set;}
    public int TaskIndex { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public TaskItem(string taskValue)
    {
      
      System.Random rand = new System.Random();

      TypeOfTask = taskValue;
      TimeRequired = rand.Next(10, 600);
      IsGoing = false;
      IsSuccesful = Convert.ToBoolean(rand.Next(0,2));
      TaskIndex = 0;
    }

     
    
  }
    
}
