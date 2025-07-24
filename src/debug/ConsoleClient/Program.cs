var haContext = await HaContextFactory.CreateHaContextAsync();

Console.WriteLine(haContext.Entity("sun.sun").State);

haContext.Entity("input_button.test_button").StateAllChanges().Subscribe(s => Console.WriteLine($"Pressed {s.New?.State}"));

await new StreamReader(Console.OpenStandardInput()).ReadLineAsync();
