using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;

namespace EzLst;

public static class LstConverter
{
    private static readonly Dictionary<string, string> mnemonicToFlagAffected = new()
    {
        // ----- Arithmetic -----
        { "ADD", "Z,S,P,C,AC" },
        { "ADI", "Z,S,P,C,AC" },
        { "ADC", "Z,S,P,C,AC" },
        { "ACI", "Z,S,P,C,AC" },
        { "SUB", "Z,S,P,C,AC" },
        { "SUI", "Z,S,P,C,AC" },
        { "SBB", "Z,S,P,C,AC" },
        { "SBI", "Z,S,P,C,AC" },
        { "INR", "Z,S,P,AC" },    // C not affected
        { "DCR", "Z,S,P,AC" },    // C not affected

        // ----- Logical -----
        { "ANA", "Z,S,P,C=0,AC=1" },
        { "ANI", "Z,S,P,C=0,AC=1" },
        { "XRA", "Z,S,P,C=0,AC=0" },
        { "XRI", "Z,S,P,C=0,AC=0" },
        { "ORA", "Z,S,P,C=0,AC=0" },
        { "ORI", "Z,S,P,C=0,AC=0" },
        { "CMP", "Z,S,P,C,AC" },
        { "CPI", "Z,S,P,C,AC" },

        // ----- Rotate -----
        { "RLC", "C" },
        { "RRC", "C" },
        { "RAL", "C" },
        { "RAR", "C" },

        // ----- Special flag operations -----
        { "CMC", "C" },
        { "STC", "C" }
    };


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
        SetAndStylizeHeaderCell(ws.Cell(1, 1), "Index");
        SetAndStylizeHeaderCell(ws.Cell(1, 2), "Alamat");
        SetAndStylizeHeaderCell(ws.Cell(1, 3), "Opcode");
        SetAndStylizeHeaderCell(ws.Cell(1, 4), "Mnemonic");
        SetAndStylizeHeaderCell(ws.Cell(1, 5), "Flag Register");
        SetAndStylizeHeaderCell(ws.Cell(1, 6), "Simbol");
        SetAndStylizeHeaderCell(ws.Cell(1, 7), "ConvertedASCII");

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

            SetAndStylizeCell(ws.Cell(row, 1), index);
            SetAndStylizeCell(ws.Cell(row, 2), address);
            SetAndStylizeCell(ws.Cell(row, 3), opcode);
            SetAndStylizeCell(ws.Cell(row, 4), mnemonic);
            SetAndStylizeCell(ws.Cell(row, 5), GetFlagFromMnemonic(mnemonic));
            SetAndStylizeCell(ws.Cell(row, 6), GetSymbolFromMnemonic(mnemonic));
            SetAndStylizeCell(ws.Cell(row, 7), ascii);

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

    private static void SetAndStylizeHeaderCell(IXLCell cell, XLCellValue value)
    {
        cell.Value = value;
        cell.Style.Font.FontName = "Aptos";
        cell.Style.Font.FontSize = 11;
        cell.Style.Font.SetFontColor(XLColor.White);
        cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#4F81BD"));
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    private static void SetAndStylizeCell(IXLCell cell, XLCellValue value)
    {
        cell.Value = value;
        cell.Style.Font.FontName = "Cascadia Code";
        cell.Style.Font.FontSize = 9;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    private static string GetSymbolFromMnemonic(string mnemonic)
    {
        // TODO: Assume LST isn't always perfect
        try
        {
            var split = mnemonic.Split(' ');
            if (split.Length > 1)
            {
                switch (split[0].ToUpper())
                {
                    // 16-bit operations
                    case "LXI":
                        var parameters = split[1].Split(',');
                        return $"{parameters[0].Trim()} <- {parameters[1].Trim()}H";
                    case "STA":
                        return $"[{split[1].Trim()}H] <- A";
                    case "LDA":
                        return $"A <- {split[1].Trim()}H";
                    case "STAX":
                        return $"[{split[1].Trim()}] <- A";
                    case "INX":
                        return $"[{split[1].Trim()}] <- [{split[1].Trim()}] + 1";
                    case "XCHG":
                        return "HL <-> DE";
                    case "SHLD":
                        return $"(addr {split[1].Trim()}H) <- L, (addr+1) <- H";
                    case "LHLD":
                        return $"L <- [{split[1].Trim()}H], H <- [{split[1].Trim()}H+1]";

                    // 8-bit operations
                    case "MOV":
                        parameters = split[1].Split(',');
                        return $"{MIfNecessary(parameters[0].Trim())} <- {MIfNecessary(parameters[1].Trim())}";
                    case "MVI":
                        parameters = split[1].Split(',');
                        return $"{parameters[0].Trim()} <- {parameters[1].Trim()}H";
                    case "ADD":
                        return $"A <- A + {split[1].Trim()}H";
                    case "ANI":
                        return $"A <- A & {split[1].Trim()}H";
                    case "SUI":
                        return $"A <- A - {split[1].Trim()}H";
                    case "CPI":
                        return $"A <- A = {split[1].Trim()}H";

                    // Jump
                    case "JMP":
                    case "JC":
                    case "JZ":
                    case "JNZ":
                        return $"-> {split[1].Trim()}";

                    default:
                        return "-";
                }
            }
            else
            {
                switch (mnemonic.ToUpper())
                {
                    case "HLT":
                        return "Stop";
                    default:
                        return "-";
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "-";
        }
    }

    private static string GetFlagFromMnemonic(string mnemonic)
    {
        // TODO: Assume LST isn't always perfect
        try
        {
            string? output = "-";
            var split = mnemonic.Split(' ');
            if (split.Length > 1)
            {
                mnemonicToFlagAffected.TryGetValue(split[0].ToUpper(), out output);
                return output ?? "-";
            }
            else
            {
                mnemonicToFlagAffected.TryGetValue(mnemonic, out output);
                return output ?? "-";
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "-";
        }
    }

    private static string MIfNecessary(string param)
    {
        if (param == "M") return "[HL]";
        return param;
    }
}