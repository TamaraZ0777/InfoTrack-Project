📘 Selenium .NET JSON-Driven Automation

📌 Overview

This project is a .NET Console application that automates a web table using Selenium WebDriver.

It:

Executes steps defined in a JSON script
Uses CSV data for test input
Performs add → edit → delete operations
Captures screenshots after each step
Handles invalid data and edge cases

⚙️ Tech Stack
.NET (C#)
Selenium WebDriver
ChromeDriver
JSON (System.Text.Json)
CSV

📄 Test Configuration
JSON (test.json)

Defines the workflow:

{
  "url": "https://demoqa.com/webtables",
  "dataFile": "data.csv",
  "steps": [
    { "action": "addRecord" },
    { "action": "waitForTableRowData" },
    { "action": "verifyInsertedRow" },
    { "action": "editSalary" },
    { "action": "waitForTableRowData" },
    { "action": "verifyUpdatedSalary" },
    { "action": "deleteRow" },
    { "action": "waitForTableRowDeletion" },
    { "action": "verifyRowDeleted" }
  ]
}

CSV (data.csv)

Each row is a test case:

FirstName,LastName,Email,Age,Salary,Department,UpdatedSalary
John,Smith,john.smith@test.com,30,5000,QA,7000

🚀 Execution Flow

For each record:

Add record
Validate insertion
Edit salary
Validate update
Delete record
Validate deletion

🧩 Custom Commands
WaitForTableRowData

Waits until a row appears:

WaitForTableRowData(firstName, lastName)
WaitForTableRowData(firstName, lastName, value)
WaitForTableRowDeletion

Waits until a row is removed.

✔ Uses occurrence counting to handle duplicates safely.

⚠️ Error Handling
Invalid Add → screenshot + skip
Invalid Edit → screenshot + cleanup (delete row)
Execution continues for next records

🔁 Duplicate Handling
Duplicate emails in CSV → warning
Deletion uses count-based validation, so duplicates don’t break the test

📸 Screenshots
Taken after each step
Stored in timestamped folder:
Screenshots/20260426_235150/
File format:
20260426_235155_01_add_clicked_John_Smith.png

📊 Summary Output

Example:

Total records: 8
Successful records: 3
Records needing review: 5
CSV warnings: 1

▶️ Run the Project
dotnet run

✅ Result
JSON-driven test execution
Data-driven testing via CSV
Robust validation & error handling
Full traceability with screenshots

💡 Notes
Each test record should ideally have a unique email
Duplicate handling is supported but may affect traceability