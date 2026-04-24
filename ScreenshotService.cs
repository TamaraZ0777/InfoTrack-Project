using OpenQA.Selenium;

public static class ScreenshotService
{
    public static string CreateRunFolder()
    {
        string projectRoot = AppDomain.CurrentDomain.BaseDirectory;

        string screenshotsRoot = Path.GetFullPath(
            Path.Combine(projectRoot, @"..\..\..\Screenshots")
        );

        string runFolderName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string screenshotsDir = Path.Combine(screenshotsRoot, runFolderName);

        Directory.CreateDirectory(screenshotsDir);

        return screenshotsDir;
    }

    public static void SaveScreenshot(IWebDriver driver, string folderPath, string fileName)
    {
        Thread.Sleep(1500);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        string fullPath = Path.Combine(folderPath, $"{fileName}_{timestamp}.png");

        var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
        screenshot.SaveAsFile(fullPath);

        Console.WriteLine($"Screenshot saved: {fullPath}");
    }
}