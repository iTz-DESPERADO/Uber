namespace Uber.Ui.Services;

public static class FileStorage
{
    public static async Task<string> SaveAsync(
        string folderName,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
        Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var finalPath = Path.Combine(folderPath, fileName);
        await using var stream = new FileStream(finalPath, FileMode.CreateNew);
        await file.CopyToAsync(stream, cancellationToken);
        return fileName;
    }

    public static void Delete(string folderName, string fileName)
    {
        var folderPath = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            folderName));
        var filePath = Path.GetFullPath(Path.Combine(folderPath, fileName));

        if (!filePath.StartsWith(folderPath + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The file path is outside the upload directory.");
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
