using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Drafty
{
    public enum State
    {
        OK = 1,
        Exit = 2,
        Error = 4,
        Restart = 8
    }

    class CounterElement
    {
        public int Value { get; set; }

        public CounterElement()
        {
            this.Value = 0;
        }

        public CounterElement(int startValue)
        {
            this.Value = startValue;
        }
    }

    class Counter
    {
        int currentElementNumber;
        List<CounterElement> lst;
        public string Separator { get; set; } = "\t";

        public string SaveDirectory { get; private set; } = @"C:\save.out";

        public bool SerializeAtTheEnd { get; set; } = true;

        public Counter()
        {
            try
            {
                Deserialize(SaveDirectory);
            }
            catch (FileNotFoundException)
            {
                AddCounter(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                currentElementNumber = 0;
            }
        }

        public void AddCounter(int countOfCounters)
        {
            if (countOfCounters <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(countOfCounters));
            }

            if (this.lst is null)
            {
                this.lst = new();
            }

            for (int i = 0; i < countOfCounters; i++)
            {
                this.lst.Add(new CounterElement());
            }
        }

        public void DeleteCounter(int counterIndex)
        {
            if (counterIndex >= 0 && counterIndex < this.lst.Count)
            {
                this.lst.RemoveAt(counterIndex);
            }
        }

        private State ButtonsHandler()
        {
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.RightArrow:
                    if (this.lst.Count != 0)
                        this.currentElementNumber = (this.currentElementNumber + 1) % this.lst.Count;
                    return State.OK;

                case ConsoleKey.LeftArrow:
                    if (this.lst.Count != 0)
                        this.currentElementNumber = Math.Abs((this.currentElementNumber - 1) % this.lst.Count);
                    return State.OK;

                case ConsoleKey.UpArrow:
                    if (currentElementNumber < this.lst.Count)
                    {
                        this.lst[currentElementNumber].Value++;
                    }
                    return State.OK;

                case ConsoleKey.DownArrow:
                    if (currentElementNumber < this.lst.Count)
                    {
                        this.lst[currentElementNumber].Value--;
                    }
                    return State.OK;

                case ConsoleKey.Escape:
                    return State.Exit;

                case ConsoleKey.N:
                    AddCounter(1);
                    return State.OK;

                case ConsoleKey.Delete:
                    DeleteCounter(currentElementNumber);
                    currentElementNumber = 0;
                    return State.OK;

                case ConsoleKey.R:
                    Restart();
                    return State.Restart;

                default:
                    return State.Error;
            }
        }

        private void Render()
        {
            Console.Write(Separator);

            for (int i = 0; i < this.lst.Count; i++)
            {
                if (i == currentElementNumber)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(this.lst[i].Value);
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(this.lst[i].Value);
                }

                Console.Write(Separator);
            }

            Console.WriteLine();
        }

        private void Serialize()
        {
            StringBuilder data = new();

            foreach (var item in this.lst)
            {
                data.Append(item.Value + " ");
            }

            File.WriteAllText(this.SaveDirectory, data.ToString());
        }

        private void Deserialize(string path)
        {
            Exception exception = null;
            int exCount = 0;
            
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));
            }

            string[] source = File.ReadAllText(path).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            AddCounter(source.Length);

            for (int i = 0; i < source.Length; i++)
            {
                try
                {
                    this.lst[i] = new CounterElement(int.Parse(source[i]));
                }
                catch (Exception ex)
                {
                    exCount++;
                    exception = ex;
                    this.lst[i] = new CounterElement(0);
                }
            }

            this.SaveDirectory = path;

            if (exCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (exCount == 1)
                {
                    Console.WriteLine("1 value was lost due save file issues.");
                }
                else
                {
                    Console.WriteLine(exCount + " values was lost due save file issues.");
                }

                Console.ResetColor();
            }
        }

        public void Run()
        {
            State state;

            do
            {
                Render();
                state = ButtonsHandler();
                Console.Clear();
            }
            while ((state & (State.Exit | State.Restart)) == 0);

            if (this.SerializeAtTheEnd && state == State.Exit)
            {
                Serialize();
                Console.Write("A");
                Console.WriteLine("App saved successfully!");
                return;
            }

            Console.WriteLine("Restarting app.");
            Console.WriteLine("Press any key...");
        }

        public void Restart()
        {
            if (File.Exists(this.SaveDirectory))
            {
                File.Delete(this.SaveDirectory);
            }
        }
    }
}
