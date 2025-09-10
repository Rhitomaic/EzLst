using System;
using System.IO;
using ClosedXML.Excel;

namespace EzLst;

public static class LstConverter
{
    public static void Convert(string inputPath, string? customOutputDirectory = null, Action<string>? progress = null)
    {
        if (File.Exists(inputPath))
        {
            if (Path.GetExtension(inputPath).Equals(".lst", StringComparison.OrdinalIgnoreCase))
            {
                progress?.Invoke($"Converting file: {Path.GetFileName(inputPath)}");
                ConvertLstToExcel(inputPath, customOutputDirectory);
            }
            else
            {
                throw new InvalidOperationException("Not an .lst file: " + inputPath);
            }
        }
        else if (Directory.Exists(inputPath))
        {
            var lstFiles = Directory.GetFiles(inputPath, "*.lst", SearchOption.TopDirectoryOnly);
            if (lstFiles.Length == 0)
                throw new InvalidOperationException("No .lst files found in directory: " + inputPath);

            foreach (var lstFile in lstFiles)
            {
                progress?.Invoke($"Converting file: {Path.GetFileName(lstFile)}");
                ConvertLstToExcel(lstFile, customOutputDirectory);
            }
        }
        else
        {
            throw new FileNotFoundException("Path not found: " + inputPath);
        }
    }

    private static void ConvertLstToExcel(string filePath, string? customOutputDirectory)
    {
        var contents = File.ReadAllLines(filePath);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("LST");

        // Header row
        ws.Cell(1, 1).Value = "Index";
        ws.Cell(1, 2).Value = "Address";
        ws.Cell(1, 3).Value = "Opcode";
        ws.Cell(1, 4).Value = "Mnemonic";
        ws.Cell(1, 5).Value = "ConvertedASCII";

        int row = 2;

        foreach (var line in contents)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 4)
                continue;

            var index = parts[0];
            var address = parts[1];
            var opcode = parts[2];
            string mnemonic;
            var ascii = "";

            if (parts.Length == 4)
            {
                mnemonic = parts[3];
            }
            else
            {
                mnemonic = string.Join(" ", parts, 3, parts.Length - 4);
                ascii = parts[^1];
            }

            ws.Cell(row, 1).Value = index;
            ws.Cell(row, 2).Value = address;
            ws.Cell(row, 3).Value = opcode;
            ws.Cell(row, 4).Value = mnemonic;
            ws.Cell(row, 5).Value = ascii;

            row++;
        }

        ws.Columns().AdjustToContents();

        var outputDir = !string.IsNullOrEmpty(customOutputDirectory)
            ? customOutputDirectory
            : Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory();

        Directory.CreateDirectory(outputDir);

        var savePath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(filePath) + ".xlsx");
        workbook.SaveAs(savePath);
    }
}