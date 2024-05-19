# FileWatcher Project

FileWatcher is a C# console application that monitors a specified folder for XML files, processes them, and moves the corresponding PDF files to an output folder.

## Cloning the repository
```
git clone https://github.com/akshaikrish/XMLFileWatcher.git
```
## Running the Main Project

To run the main project, follow these steps:

1. Open a terminal or command prompt and navigate to the project directory by running:
    ```
    cd XMLFileWatcher/FileWatcher
    ```
2. Edit the `appSettings.json` file in the project directory to set the appropriate paths for the input and output folders.


3. Build the project using the following command:
    ```
    dotnet build
    ```

4. Run the project with the following command:
    ```
    dotnet run
    ```
5. Program logs are available at `XMLFileWatcher/FileWatcher/bin/Debug/net8.0/filewatcher.log`.
    

## Running the Test Project

1. Navigate to the test project directory:
    ```
    cd XMLFileWatcher/FileWatcherTest
    ```

2. Build the project using the following command:
    ```
    dotnet build
    ```

3. Run the project using the following command:
    ```
    dotnet test
    ```

## Decisions and Observations

1. **Use of NLog for Logging**: NLog was chosen for logging due to its flexibility, configurability, and performance. It allows logging to various targets, including files and the console, and provides support for log level filtering.


2. **Configuration with appsettings.json**: Configuration settings such as input and output folder paths are stored in the `appsettings.json` file. This allows for easy modification of settings without modifying the code.


3. **FileWatcher Implementation**: The `FileSystemWatcher` class from the .NET Framework was used to monitor the input folder for new XML files. This provides a lightweight and efficient way to react to file system changes.


4. **Testing with xUnit**: Unit tests were written using xUnit as the testing framework. This allows for isolated testing of individual components and ensures that the application behaves as expected under different scenarios.


5. **Error Handling**: Error handling was implemented to handle exceptions gracefully and provide meaningful error messages in the logs. This helps in diagnosing issues and troubleshooting problems that may arise during runtime.


6. **Regular Expression for MRN Validation**: A regular expression was used to validate the Medical Record Number (MRN) format in the XML files. This ensures that only valid MRNs are processed and prevents processing of invalid data.


7. **Observations**: During development, it was observed that proper error handling and logging are crucial for the robustness and maintainability of the application. Additionally, thorough testing helped uncover edge cases and potential issues early in the development process, leading to a more reliable final product.

