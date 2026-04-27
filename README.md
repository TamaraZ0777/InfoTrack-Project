# 📘 Selenium .NET JSON-Driven Automation

## 📌 Overview

This project is a .NET Console application that automates a web table using Selenium WebDriver.

It:

- Executes steps defined in a JSON script  
- Uses CSV data for test input  
- Performs add → edit → delete operations  
- Captures screenshots after each step  
- Handles invalid data and edge cases  

---

## ⚙️ Tech Stack

- .NET (C#)  
- Selenium WebDriver  
- ChromeDriver  
- JSON (System.Text.Json)  
- CSV  

---

## 📄 Test Configuration

### JSON (test.json)

Defines the workflow:

```json
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
```
---

### CSV (`data.csv`)

Each row in the CSV file represents one test case.

Example:

```csv
FirstName,LastName,Email,Age,Salary,Department,UpdatedSalary
John,Smith,john.smith@test.com,30,5000,QA,7000
Emma,Johnson,emma.johnson@test.com,28,6000,HR,8000
```

---

## 🚀 Execution Flow

For each record from the CSV file, the application performs the following steps:

1. Add a new record
2. Wait for the row to appear in the table
3. Verify that the inserted row exists
4. Edit the salary value
5. Wait for the updated row data
6. Verify that the updated salary is displayed
7. Delete the row
8. Wait for the row deletion
9. Verify that the row no longer exists

---

## 🧩 Custom Commands

### `WaitForTableRowData`

Waits until the expected row data is visible in the table.

Examples:

```csharp
WaitForTableRowData(firstName, lastName);
WaitForTableRowData(firstName, lastName, value);
```

---

### `WaitForTableRowDeletion`

Waits until the expected row is removed from the table.

The deletion check uses occurrence counting to handle duplicate records more safely.

---

## ⚠️ Error Handling

The application handles invalid data without stopping the whole execution.

- Invalid data during Add step → screenshot is saved, the record is marked for review, and the next record is processed
- Invalid data during Edit step → screenshot is saved, the modal is closed, the created row is cleaned up, and the next record is processed
- Malformed CSV rows → warning is shown in the console and included in the final summary

---

## 🔁 Duplicate Handling

Duplicate emails in the CSV file are detected during CSV parsing.

The application prints a warning, but the execution continues.

Deletion validation uses occurrence counting, so duplicate records already available in the table do not break the test flow.

---

## 📸 Screenshots

Screenshots are saved after each of the main execution steps.

They are stored in a timestamped folder:

```text
Screenshots/20260426_235150/
```

Screenshot file names start with a timestamp so they can be sorted chronologically.
Screenshot file names contain the step name and the first name and the last name of the record being processed:

```text
20260426_235155_712_01_add_clicked_John_Smith.png
```

---

## 📊 Summary Output

At the end of the run, the application prints a summary.

Example:

```text
Run summary:
Total records: 8
Successful records: 3
Records needing review: 5
Screenshots folder: D:\InfoTrack_Test Task\SeleniumRunner\Screenshots\20260426_235150
CSV CSV warning details:
- Duplicate email found in CSV: duplicate@test.com appears 2 times.
```

---

## ▶️ Run the Project

Restore dependencies:

```bash
dotnet restore
```

Run the application:

```bash
dotnet run
```

---

## 📋 Prerequisites

- .NET SDK installed
- Google Chrome installed
- Internet connection

---

## ✅ Result

The solution provides:

- JSON-driven test execution
- CSV-based test data
- Add, edit, and delete automation
- Custom wait commands
- Screenshot logging
- Invalid data handling
- Duplicate data handling
- Final execution summary

---

## 💡 Notes

- Each test record should ideally have a unique email address, but the requirement needs confirmation depending on the test goals
- Duplicate records are supported, but unique test data is easier to review
- CSV warnings (e.g. duplicate emails) are shown in the console
- Screenshots are saved automatically during execution
- Screenshots are not committed to the repository
- Based on the observed behaviour and the implemented logic, the webpage table is not expected to have more than 10 records