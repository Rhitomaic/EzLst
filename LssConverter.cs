using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class AvrInstructionInfo
{
    public string Mnemonic { get; set; }
    public string Operands { get; set; }
    public string Description { get; set; }

    // Option 1: Dynamic operation generator
    public Func<string, string>? OperationFunc { get; set; }

    // Option 2: Keep static fallback
    public string? OperationTemplate { get; set; }

    public string FlagsAffected { get; set; }

    public string GetOperation(string line)
        => OperationFunc?.Invoke(line) ?? OperationTemplate ?? string.Empty;
}

public static class AvrInstructionTable
{
    public static readonly Dictionary<string, AvrInstructionInfo> Mnemonics = new()
    {
        
        ["ADD"] = new AvrInstructionInfo
        {
            Mnemonic = "ADD",
            Operands = "Rd, Rr",
            Description = "Add without Carry",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} + {parts[2]}";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["ADC"] = new AvrInstructionInfo
        {
            Mnemonic = "ADC",
            Operands = "Rd, Rr",
            Description = "Add with Carry",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} + {parts[2]} + C";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["ADIW"] = new AvrInstructionInfo
        {
            Mnemonic = "ADIW",
            Operands = "Rd, K",
            Description = "Add Immediate to Word",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"R[{parts[1]}+1]:{parts[1]} ← R[{parts[1]}+1]:{parts[1]} + {parts[2]}";
            },
            FlagsAffected = "Z,C,N,V,S"
        },
        ["SUB"] = new AvrInstructionInfo
        {
            Mnemonic = "SUB",
            Operands = "Rd, Rr",
            Description = "Subtract without Carry",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} - {parts[2]}";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["SUBI"] = new AvrInstructionInfo
        {
            Mnemonic = "SUBI",
            Operands = "Rd, K",
            Description = "Subtract Immediate",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} - {parts[2]}";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["SBC"] = new AvrInstructionInfo
        {
            Mnemonic = "SBC",
            Operands = "Rd, Rr",
            Description = "Subtract with Carry",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} - {parts[2]} - C";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["SBCI"] = new AvrInstructionInfo
        {
            Mnemonic = "SBCI",
            Operands = "Rd, K",
            Description = "Subtract Immediate with Carry",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} - {parts[2]} - C";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["SBIW"] = new AvrInstructionInfo
        {
            Mnemonic = "SBIW",
            Operands = "Rd, K",
            Description = "Subtract Immediate from Word",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"R[{parts[1]}+1]:{parts[1]} ← R[{parts[1]}+1]:{parts[1]} - {parts[2]}";
            },
            FlagsAffected = "Z,C,N,V,S"
        },
        ["AND"] = new AvrInstructionInfo
        {
            Mnemonic = "AND",
            Operands = "Rd, Rr",
            Description = "Logical AND",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} ∧ {parts[2]}";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["ANDI"] = new AvrInstructionInfo
        {
            Mnemonic = "ANDI",
            Operands = "Rd, K",
            Description = "Logical AND with Immediate",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} ∧ {parts[2]}";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["OR"] = new AvrInstructionInfo
        {
            Mnemonic = "OR",
            Operands = "Rd, Rr",
            Description = "Logical OR",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} ∨ {parts[2]}";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["ORI"] = new AvrInstructionInfo
        {
            Mnemonic = "ORI",
            Operands = "Rd, K",
            Description = "Logical OR with Immediate",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} ∨ {parts[2]}";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["EOR"] = new AvrInstructionInfo
        {
            Mnemonic = "EOR",
            Operands = "Rd, Rr",
            Description = "Exclusive OR",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} ⊕ {parts[2]}";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["COM"] = new AvrInstructionInfo
        {
            Mnemonic = "COM",
            Operands = "Rd",
            Description = "One’s Complement",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"{parts[1]} ← 0xFF - {parts[1]}";
            },
            FlagsAffected = "Z,C,N,V,S"
        },
        ["NEG"] = new AvrInstructionInfo
        {
            Mnemonic = "NEG",
            Operands = "Rd",
            Description = "Two’s Complement",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"{parts[1]} ← 0x00 - {parts[1]}";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["SBR"] = new AvrInstructionInfo
        {
            Mnemonic = "SBR",
            Operands = "Rd, K",
            Description = "Set Bit(s) in Register",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} ∨ {parts[2]}";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["CBR"] = new AvrInstructionInfo
        {
            Mnemonic = "CBR",
            Operands = "Rd, K",
            Description = "Clear Bit(s) in Register",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} ← {parts[1]} ∧ (0xFF - {parts[2]})";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["INC"] = new AvrInstructionInfo
        {
            Mnemonic = "INC",
            Operands = "Rd",
            Description = "Increment",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"{parts[1]} ← {parts[1]} + 1";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["DEC"] = new AvrInstructionInfo
        {
            Mnemonic = "DEC",
            Operands = "Rd",
            Description = "Decrement",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"{parts[1]} ← {parts[1]} - 1";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["TST"] = new AvrInstructionInfo
        {
            Mnemonic = "TST",
            Operands = "Rd",
            Description = "Test for Zero or Minus",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"{parts[1]} ← {parts[1]} ∧ {parts[1]}";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["CLR"] = new AvrInstructionInfo
        {
            Mnemonic = "CLR",
            Operands = "Rd",
            Description = "Clear Register",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"{parts[1]} ← {parts[1]} ⊕ {parts[1]}";
            },
            FlagsAffected = "Z,N,V,S"
        },
        ["SER"] = new AvrInstructionInfo
        {
            Mnemonic = "SER",
            Operands = "Rd",
            Description = "Set Register",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"{parts[1]} ← 0xFF";
            },
            FlagsAffected = "None"
        },
        ["MUL"] = new AvrInstructionInfo
        {
            Mnemonic = "MUL",
            Operands = "Rd, Rr",
            Description = "Multiply Unsigned",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"R1:R0 ← {parts[1]} × {parts[2]}";
            },
            FlagsAffected = "Z,C"
        },
        ["MULS"] = new AvrInstructionInfo
        {
            Mnemonic = "MULS",
            Operands = "Rd, Rr",
            Description = "Multiply Signed",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"R1:R0 ← {parts[1]} × {parts[2]}";
            },
            FlagsAffected = "Z,C"
        },
        ["MULSU"] = new AvrInstructionInfo
        {
            Mnemonic = "MULSU",
            Operands = "Rd, Rr",
            Description = "Multiply Signed with Unsigned",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"R1:R0 ← {parts[1]} × {parts[2]}";
            },
            FlagsAffected = "Z,C"
        },
        ["FMUL"] = new AvrInstructionInfo
        {
            Mnemonic = "FMUL",
            Operands = "Rd, Rr",
            Description = "Fractional Multiply Unsigned",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"R1:R0 ← ({parts[1]} × {parts[2]}) << 1";
            },
            FlagsAffected = "Z,C"
        },
        ["FMULS"] = new AvrInstructionInfo
        {
            Mnemonic = "FMULS",
            Operands = "Rd, Rr",
            Description = "Fractional Multiply Signed",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"R1:R0 ← ({parts[1]} × {parts[2]}) << 1";
            },
            FlagsAffected = "Z,C"
        },
        ["FMULSU"] = new AvrInstructionInfo
        {
            Mnemonic = "FMULSU",
            Operands = "Rd, Rr",
            Description = "Fractional Multiply Signed/Unsigned",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"R1:R0 ← ({parts[1]} × {parts[2]}) << 1";
            },
            FlagsAffected = "Z,C"
        },
        ["DES"] = new AvrInstructionInfo
        {
            Mnemonic = "DES",
            Operands = "K",
            Description = "Data Encryption",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"Encrypt/Decrypt(R15:R0, {parts[1]})";
            },
            FlagsAffected = "None"
        },
        
        // Uh, branch?
         ["RJMP"] = new AvrInstructionInfo
        {
            Mnemonic = "RJMP",
            Operands = "k",
            Description = "Relative Jump",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["IJMP"] = new AvrInstructionInfo
        {
            Mnemonic = "IJMP",
            Operands = "—",
            Description = "Indirect Jump (Z)",
            OperationFunc = mnemonic =>
            {
                return "PC ← Z";
            },
            FlagsAffected = "None"
        },
        ["EIJMP"] = new AvrInstructionInfo
        {
            Mnemonic = "EIJMP",
            Operands = "—",
            Description = "Extended Indirect Jump",
            OperationFunc = mnemonic =>
            {
                return "PC ← EIND:Z";
            },
            FlagsAffected = "None"
        },
        ["JMP"] = new AvrInstructionInfo
        {
            Mnemonic = "JMP",
            Operands = "k",
            Description = "Jump",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"PC ← {parts[1]}";
            },
            FlagsAffected = "None"
        },
        ["RCALL"] = new AvrInstructionInfo
        {
            Mnemonic = "RCALL",
            Operands = "k",
            Description = "Relative Call Subroutine",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["ICALL"] = new AvrInstructionInfo
        {
            Mnemonic = "ICALL",
            Operands = "—",
            Description = "Indirect Call (Z)",
            OperationFunc = mnemonic =>
            {
                return "PC ← Z";
            },
            FlagsAffected = "None"
        },
        ["EICALL"] = new AvrInstructionInfo
        {
            Mnemonic = "EICALL",
            Operands = "—",
            Description = "Extended Indirect Call",
            OperationFunc = mnemonic =>
            {
                return "PC ← EIND:Z";
            },
            FlagsAffected = "None"
        },
        ["CALL"] = new AvrInstructionInfo
        {
            Mnemonic = "CALL",
            Operands = "k",
            Description = "Call Subroutine",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                return $"PC ← {parts[1]}";
            },
            FlagsAffected = "None"
        },
        ["RET"] = new AvrInstructionInfo
        {
            Mnemonic = "RET",
            Operands = "—",
            Description = "Subroutine Return",
            OperationFunc = mnemonic =>
            {
                return "PC ← STACK";
            },
            FlagsAffected = "None"
        },
        ["RETI"] = new AvrInstructionInfo
        {
            Mnemonic = "RETI",
            Operands = "—",
            Description = "Interrupt Return",
            OperationFunc = mnemonic =>
            {
                return "PC ← STACK";
            },
            FlagsAffected = "I"
        },
        ["CPSE"] = new AvrInstructionInfo
        {
            Mnemonic = "CPSE",
            Operands = "Rd, Rr",
            Description = "Compare, Skip if Equal",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"if ({parts[1]} == {parts[2]}) skip next";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["CP"] = new AvrInstructionInfo
        {
            Mnemonic = "CP",
            Operands = "Rd, Rr",
            Description = "Compare",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} - {parts[2]}";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["CPC"] = new AvrInstructionInfo
        {
            Mnemonic = "CPC",
            Operands = "Rd, Rr",
            Description = "Compare with Carry",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} - {parts[2]} - C";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["CPI"] = new AvrInstructionInfo
        {
            Mnemonic = "CPI",
            Operands = "Rd, K",
            Description = "Compare with Immediate",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;
                return $"{parts[1]} - {parts[2]}";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        
        
    };
}

public class LssInstruction
{
    public string Section { get; set; }
    public string Address { get; set; }
    public string Opcode { get; set; }
    public string Mnemonic { get; set; }
    public string FlagsAffected { get; set; }
    public string Symbol { get; set; }
    public string Comment { get; set; }

    public override string ToString()
    {
        return
            $"{Section,-8} {Address,-6} {Opcode,-10} {Mnemonic,-35} | {FlagsAffected,-10} | {Symbol,-30} | {Comment}";
    }
}

public class LssParser
{
    private static readonly Regex SectionRegex = new(@"\.(cseg|dseg|eseg)", RegexOptions.IgnoreCase);

    private static readonly Regex CodeLineRegex =
        new(@"^\s*([0-9A-Fa-f]{6})\s+([0-9A-Fa-f ]{4,10})\s+([a-zA-Z].*?)\s*(?:;\s*(.*))?$", RegexOptions.Compiled);

    public List<LssInstruction> Parse(string filePath)
    {
        var result = new List<LssInstruction>();
        var lines = File.ReadAllLines(filePath);
        string currentSection = "";
        bool inCode = false;

        foreach (var line in lines)
        {
            var secMatch = SectionRegex.Match(line);
            if (secMatch.Success)
            {
                currentSection = secMatch.Value;
                inCode = currentSection.Equals(".cseg", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inCode) continue; // Skip until code section

            var match = CodeLineRegex.Match(line);
            if (match.Success)
            {
                var address = match.Groups[1].Value.Trim();
                var opcode = match.Groups[2].Value.Trim();
                var mnemonic = match.Groups[3].Value.Trim();
                var comment = match.Groups[4].Value.Trim();

                var mnemonicKey = mnemonic.Split(' ')[0].ToUpper();

                string flags = "None", symbol = "???";
                if (AvrInstructionTable.Mnemonics.TryGetValue(mnemonicKey, out var info))
                {
                    flags = info.FlagsAffected;
                    symbol = info.GetOperation(mnemonic);
                }

                result.Add(new LssInstruction
                {
                    Section = currentSection,
                    Address = address,
                    Opcode = opcode,
                    Mnemonic = mnemonic,
                    FlagsAffected = flags,
                    Symbol = symbol,
                    Comment = comment
                });
            }
        }

        return result;
    }
}

public class LssConverter
{
    public static void Convert(string inputPath, string? customOutputDirectory = null, Action<string>? progress = null)
    {
        if (File.Exists(inputPath))
        {
            if (Path.GetExtension(inputPath).Equals(".lss", StringComparison.OrdinalIgnoreCase))
            {
                progress?.Invoke($"Converting file: {Path.GetFileName(inputPath)}");
                var parser = new LssParser();
                var instructions = parser.Parse(inputPath);
                Console.WriteLine(
                    "Section   Address Opcode     Mnemonic                           | Flags      | Symbol");
                Console.WriteLine(
                    "---------------------------------------------------------------------------------------");
                foreach (var instr in instructions)
                {
                    Console.WriteLine(instr.ToString());
                }
            }
            else
            {
                throw new InvalidOperationException("Not an .lst file: " + inputPath);
            }
        }
        else if (Directory.Exists(inputPath))
        {
            var lssFiles = Directory.GetFiles(inputPath, "*.lss", SearchOption.TopDirectoryOnly);
            if (lssFiles.Length == 0)
                throw new InvalidOperationException("No .lss files found in directory: " + inputPath);

            foreach (var lstFile in lssFiles)
            {
                progress?.Invoke($"Converting file: {Path.GetFileName(lstFile)}");
                var parser = new LssParser();
                var instructions = parser.Parse(inputPath);
                Console.WriteLine(
                    "Section   Address Opcode     Mnemonic                           | Flags      | Symbol");
                Console.WriteLine(
                    "---------------------------------------------------------------------------------------");
                foreach (var instr in instructions)
                {
                    Console.WriteLine(instr.ToString());
                }
            }
        }
        else
        {
            throw new FileNotFoundException("Path not found: " + inputPath);
        }
    }
}