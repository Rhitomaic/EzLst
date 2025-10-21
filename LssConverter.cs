using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class AvrInstructionInfo
{
    public string Mnemonic { get; set; }
    public string Operands { get; set; }
    public string Description { get; set; }
    public string Operation { get; set; }
    public string FlagsAffected { get; set; }
}

public static class AvrInstructionTable
{
    public static readonly Dictionary<string, AvrInstructionInfo> Mnemonics = new()
    {
        ["ADD"] = new AvrInstructionInfo
        {
            Mnemonic = "ADD",
            Operands = "Rd, Rr",
            Description = "Add without carry",
            Operation = "Rd ← Rd + Rr",
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["ADC"] = new AvrInstructionInfo
        {
            Mnemonic = "ADC",
            Operands = "Rd, Rr",
            Description = "Add with carry",
            Operation = "Rd ← Rd + Rr + C",
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["ADIW"] = new AvrInstructionInfo
        {
            Mnemonic = "ADIW",
            Operands = "Rd+1:Rd, K",
            Description = "Add immediate to word",
            Operation = "R[d+1]:Rd ← R[d+1]:Rd + K",
            FlagsAffected = "Z,C,N,V,S"
        },
        ["SUB"] = new AvrInstructionInfo
        {
            Mnemonic = "SUB",
            Operands = "Rd, Rr",
            Description = "Subtract without carry",
            Operation = "Rd ← Rd - Rr",
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["SUBI"] = new AvrInstructionInfo
        {
            Mnemonic = "SUBI",
            Operands = "Rd, K",
            Description = "Subtract immediate",
            Operation = "Rd ← Rd - K",
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["SBC"] = new AvrInstructionInfo
        {
            Mnemonic = "SBC",
            Operands = "Rd, Rr",
            Description = "Subtract with carry",
            Operation = "Rd ← Rd - Rr - C",
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["SBCI"] = new AvrInstructionInfo
        {
            Mnemonic = "SBCI",
            Operands = "Rd, K",
            Description = "Subtract immediate with carry",
            Operation = "Rd ← Rd - K - C",
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["AND"] = new AvrInstructionInfo
        {
            Mnemonic = "AND",
            Operands = "Rd, Rr",
            Description = "Logical AND",
            Operation = "Rd ← Rd ∧ Rr",
            FlagsAffected = "Z,N,V,S"
        },
        ["ANDI"] = new AvrInstructionInfo
        {
            Mnemonic = "ANDI",
            Operands = "Rd, K",
            Description = "Logical AND with immediate",
            Operation = "Rd ← Rd ∧ K",
            FlagsAffected = "Z,N,V,S"
        },
        ["OR"] = new AvrInstructionInfo
        {
            Mnemonic = "OR",
            Operands = "Rd, Rr",
            Description = "Logical OR",
            Operation = "Rd ← Rd ∨ Rr",
            FlagsAffected = "Z,N,V,S"
        },
        ["EOR"] = new AvrInstructionInfo
        {
            Mnemonic = "EOR",
            Operands = "Rd, Rr",
            Description = "Exclusive OR",
            Operation = "Rd ← Rd ⊕ Rr",
            FlagsAffected = "Z,N,V,S"
        },
        ["COM"] = new AvrInstructionInfo
        {
            Mnemonic = "COM",
            Operands = "Rd",
            Description = "One’s complement",
            Operation = "Rd ← 0xFF - Rd",
            FlagsAffected = "Z,C,N,V,S"
        },
        ["NEG"] = new AvrInstructionInfo
        {
            Mnemonic = "NEG",
            Operands = "Rd",
            Description = "Two’s complement",
            Operation = "Rd ← 0x00 - Rd",
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["SBR"] = new AvrInstructionInfo
        {
            Mnemonic = "SBR",
            Operands = "Rd, K",
            Description = "Set bit(s) in register",
            Operation = "Rd ← Rd ∨ K",
            FlagsAffected = "Z,N,V,S"
        },
        ["CBR"] = new AvrInstructionInfo
        {
            Mnemonic = "CBR",
            Operands = "Rd, K",
            Description = "Clear bit(s) in register",
            Operation = "Rd ← Rd ∧ (0xFF - K)",
            FlagsAffected = "Z,N,V,S"
        },
        ["INC"] = new AvrInstructionInfo
        {
            Mnemonic = "INC",
            Operands = "Rd",
            Description = "Increment",
            Operation = "Rd ← Rd + 1",
            FlagsAffected = "Z,N,V,S"
        },
        ["DEC"] = new AvrInstructionInfo
        {
            Mnemonic = "DEC",
            Operands = "Rd",
            Description = "Decrement",
            Operation = "Rd ← Rd - 1",
            FlagsAffected = "Z,N,V,S"
        },
        ["TST"] = new AvrInstructionInfo
        {
            Mnemonic = "TST",
            Operands = "Rd",
            Description = "Test for zero or minus",
            Operation = "Rd ← Rd ∧ Rd",
            FlagsAffected = "Z,N,V,S"
        },
        ["CLR"] = new AvrInstructionInfo
        {
            Mnemonic = "CLR",
            Operands = "Rd",
            Description = "Clear register",
            Operation = "Rd ← Rd ⊕ Rd",
            FlagsAffected = "Z,N,V,S"
        },
        ["SER"] = new AvrInstructionInfo
        {
            Mnemonic = "SER",
            Operands = "Rd",
            Description = "Set register",
            Operation = "Rd ← 0xFF",
            FlagsAffected = "None"
        },
        ["MUL"] = new AvrInstructionInfo
        {
            Mnemonic = "MUL",
            Operands = "Rd, Rr",
            Description = "Multiply Unsigned",
            Operation = "R1:R0 ← Rd × Rr (UU)",
            FlagsAffected = "Z,C"
        },
        ["MULS"] = new AvrInstructionInfo
        {
            Mnemonic = "MULS",
            Operands = "Rd, Rr",
            Description = "Multiply Signed",
            Operation = "R1:R0 ← Rd × Rr (SS)",
            FlagsAffected = "Z,C"
        },
        ["MULSU"] = new AvrInstructionInfo
        {
            Mnemonic = "MULSU",
            Operands = "Rd, Rr",
            Description = "Multiply Signed with Unsigned",
            Operation = "R1:R0 ← Rd × Rr (SU)",
            FlagsAffected = "Z,C"
        },
        ["FMUL"] = new AvrInstructionInfo
        {
            Mnemonic = "FMUL",
            Operands = "Rd, Rr",
            Description = "Fractional Multiply Unsigned",
            Operation = "R1:R0 ← (Rd × Rr) << 1 (UU)",
            FlagsAffected = "Z,C"
        },
        ["FMULS"] = new AvrInstructionInfo
        {
            Mnemonic = "FMULS",
            Operands = "Rd, Rr",
            Description = "Fractional Multiply Signed",
            Operation = "R1:R0 ← (Rd × Rr) << 1 (SS)",
            FlagsAffected = "Z,C"
        },
        ["FMULSU"] = new AvrInstructionInfo
        {
            Mnemonic = "FMULSU",
            Operands = "Rd, Rr",
            Description = "Fractional Multiply Signed with Unsigned",
            Operation = "R1:R0 ← (Rd × Rr) << 1 (SU)",
            FlagsAffected = "Z,C"
        },
        ["DES"] = new AvrInstructionInfo
        {
            Mnemonic = "DES",
            Operands = "K",
            Description = "Data Encryption / Decryption",
            Operation = "if (H=0) then R15:R0 ← Encrypt(R15:R0, K) else R15:R0 ← Decrypt(R15:R0, K)",
            FlagsAffected = "None"
        },
        ["RJMP"] = new AvrInstructionInfo
        {
            Mnemonic = "RJMP",
            Operands = "k",
            Description = "Relative Jump",
            Operation = "PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["IJMP"] = new AvrInstructionInfo
        {
            Mnemonic = "IJMP",
            Operands = "—",
            Description = "Indirect Jump (Z)",
            Operation = "PC ← Z",
            FlagsAffected = "None"
        },
        ["EIJMP"] = new AvrInstructionInfo
        {
            Mnemonic = "EIJMP",
            Operands = "—",
            Description = "Extended Indirect Jump",
            Operation = "PC ← EIND:Z",
            FlagsAffected = "None"
        },
        ["JMP"] = new AvrInstructionInfo
        {
            Mnemonic = "JMP",
            Operands = "k",
            Description = "Jump",
            Operation = "PC ← k",
            FlagsAffected = "None"
        },
        ["RCALL"] = new AvrInstructionInfo
        {
            Mnemonic = "RCALL",
            Operands = "k",
            Description = "Relative Call Subroutine",
            Operation = "PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["ICALL"] = new AvrInstructionInfo
        {
            Mnemonic = "ICALL",
            Operands = "—",
            Description = "Indirect Call (Z)",
            Operation = "PC ← Z",
            FlagsAffected = "None"
        },
        ["EICALL"] = new AvrInstructionInfo
        {
            Mnemonic = "EICALL",
            Operands = "—",
            Description = "Extended Indirect Call",
            Operation = "PC ← EIND:Z",
            FlagsAffected = "None"
        },
        ["CALL"] = new AvrInstructionInfo
        {
            Mnemonic = "CALL",
            Operands = "k",
            Description = "Call Subroutine",
            Operation = "PC ← k",
            FlagsAffected = "None"
        },
        ["RET"] = new AvrInstructionInfo
        {
            Mnemonic = "RET",
            Operands = "—",
            Description = "Subroutine Return",
            Operation = "PC ← STACK",
            FlagsAffected = "None"
        },
        ["RETI"] = new AvrInstructionInfo
        {
            Mnemonic = "RETI",
            Operands = "—",
            Description = "Interrupt Return",
            Operation = "PC ← STACK",
            FlagsAffected = "I"
        },

// Compare and Skip Instructions
        ["CPSE"] = new AvrInstructionInfo
        {
            Mnemonic = "CPSE",
            Operands = "Rd, Rr",
            Description = "Compare, Skip if Equal",
            Operation = "if (Rd == Rr) skip next instruction",
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["CP"] = new AvrInstructionInfo
        {
            Mnemonic = "CP",
            Operands = "Rd, Rr",
            Description = "Compare",
            Operation = "Rd - Rr",
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["CPC"] = new AvrInstructionInfo
        {
            Mnemonic = "CPC",
            Operands = "Rd, Rr",
            Description = "Compare with Carry",
            Operation = "Rd - Rr - C",
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["CPI"] = new AvrInstructionInfo
        {
            Mnemonic = "CPI",
            Operands = "Rd, K",
            Description = "Compare with Immediate",
            Operation = "Rd - K",
            FlagsAffected = "Z,C,N,V,S,H"
        },

        ["SBRC"] = new AvrInstructionInfo
        {
            Mnemonic = "SBRC",
            Operands = "Rr,b",
            Description = "Skip if Bit Cleared",
            Operation = "if (Rr(b) == 0) skip next instruction",
            FlagsAffected = "None"
        },
        ["SBRS"] = new AvrInstructionInfo
        {
            Mnemonic = "SBRS",
            Operands = "Rr,b",
            Description = "Skip if Bit Set",
            Operation = "if (Rr(b) == 1) skip next instruction",
            FlagsAffected = "None"
        },
        ["SBIC"] = new AvrInstructionInfo
        {
            Mnemonic = "SBIC",
            Operands = "A,b",
            Description = "Skip if Bit in I/O Cleared",
            Operation = "if (I/O(A,b) == 0) skip next instruction",
            FlagsAffected = "None"
        },
        ["SBIS"] = new AvrInstructionInfo
        {
            Mnemonic = "SBIS",
            Operands = "A,b",
            Description = "Skip if Bit in I/O Set",
            Operation = "if (I/O(A,b) == 1) skip next instruction",
            FlagsAffected = "None"
        },

// Branch Instructions
        ["BRBS"] = new AvrInstructionInfo
        {
            Mnemonic = "BRBS",
            Operands = "s,k",
            Description = "Branch if Flag Set",
            Operation = "if (SREG(s) == 1) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRBC"] = new AvrInstructionInfo
        {
            Mnemonic = "BRBC",
            Operands = "s,k",
            Description = "Branch if Flag Cleared",
            Operation = "if (SREG(s) == 0) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BREQ"] = new AvrInstructionInfo
        {
            Mnemonic = "BREQ",
            Operands = "k",
            Description = "Branch if Equal",
            Operation = "if (Z == 1) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRNE"] = new AvrInstructionInfo
        {
            Mnemonic = "BRNE",
            Operands = "k",
            Description = "Branch if Not Equal",
            Operation = "if (Z == 0) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRCS"] = new AvrInstructionInfo
        {
            Mnemonic = "BRCS",
            Operands = "k",
            Description = "Branch if Carry Set",
            Operation = "if (C == 1) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRCC"] = new AvrInstructionInfo
        {
            Mnemonic = "BRCC",
            Operands = "k",
            Description = "Branch if Carry Cleared",
            Operation = "if (C == 0) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRMI"] = new AvrInstructionInfo
        {
            Mnemonic = "BRMI",
            Operands = "k",
            Description = "Branch if Minus",
            Operation = "if (N == 1) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRPL"] = new AvrInstructionInfo
        {
            Mnemonic = "BRPL",
            Operands = "k",
            Description = "Branch if Plus",
            Operation = "if (N == 0) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRGE"] = new AvrInstructionInfo
        {
            Mnemonic = "BRGE",
            Operands = "k",
            Description = "Branch if ≥ (Signed)",
            Operation = "if (S == 0) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRLT"] = new AvrInstructionInfo
        {
            Mnemonic = "BRLT",
            Operands = "k",
            Description = "Branch if < (Signed)",
            Operation = "if (S == 1) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRHS"] = new AvrInstructionInfo
        {
            Mnemonic = "BRHS",
            Operands = "k",
            Description = "Branch if Half Carry Set",
            Operation = "if (H == 1) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRHC"] = new AvrInstructionInfo
        {
            Mnemonic = "BRHC",
            Operands = "k",
            Description = "Branch if Half Carry Cleared",
            Operation = "if (H == 0) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRIE"] = new AvrInstructionInfo
        {
            Mnemonic = "BRIE",
            Operands = "k",
            Description = "Branch if Interrupt Enabled",
            Operation = "if (I == 1) PC ← PC + k + 1",
            FlagsAffected = "None"
        },
        ["BRID"] = new AvrInstructionInfo
        {
            Mnemonic = "BRID",
            Operands = "k",
            Description = "Branch if Interrupt Disabled",
            Operation = "if (I == 0) PC ← PC + k + 1",
            FlagsAffected = "None"
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
                    symbol = info.Operation;
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

    private string GetFlagsAffected(string mnemonic)
    {
        mnemonic = mnemonic.ToLower();
        if (mnemonic.StartsWith("add")) return "Z,N,V,C";
        if (mnemonic.StartsWith("adc")) return "Z,N,V,C";
        if (mnemonic.StartsWith("sub")) return "Z,N,V,C";
        if (mnemonic.StartsWith("ldi")) return "None";
        if (mnemonic.StartsWith("mov")) return "None";
        if (mnemonic.StartsWith("com")) return "Z,N,V";
        if (mnemonic.StartsWith("dec")) return "Z,N,V";
        if (mnemonic.StartsWith("brne")) return "None";
        if (mnemonic.StartsWith("lpm")) return "None";
        return "Unknown";
    }

    private string GetSymbolMeaning(string mnemonic)
    {
        var parts = mnemonic.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return mnemonic;

        switch (parts[0].ToLower())
        {
            case "ldi":
                return $"{parts[1].ToUpper()} ← {parts[2]}";
            case "mov":
                return $"{parts[1].ToUpper()} ← {parts[2].ToUpper()}";
            case "add":
                return $"{parts[1].ToUpper()} ← {parts[1].ToUpper()} + {parts[2].ToUpper()}";
            case "sub":
                return $"{parts[1].ToUpper()} ← {parts[1].ToUpper()} - {parts[2].ToUpper()}";
            case "dec":
                return $"{parts[1].ToUpper()} ← {parts[1].ToUpper()} - 1";
            case "com":
                return $"{parts[1].ToUpper()} ← NOT {parts[1].ToUpper()}";
            case "out":
                return $"{parts[2].ToUpper()} ← {parts[1].ToUpper()}";
            case "in":
                return $"{parts[1].ToUpper()} ← {parts[2].ToUpper()}";
            default:
                return mnemonic;
        }
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