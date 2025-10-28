using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace EzLst;

public class AvrInstructionInfo
{
    public string Mnemonic { get; init; } = string.Empty;
    public string Operands { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    // Option 1: Dynamic operation generator
    public Func<string, string>? OperationFunc { get; init; }

    public string FlagsAffected { get; init; } = string.Empty;

    public string GetOperation(string line)
        => OperationFunc?.Invoke(line) ?? string.Empty;
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} + {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} + {parts[2]} + C";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3
                    ? mnemonic
                    : $"R[{parts[1]}+1]:{parts[1]} ← R[{parts[1]}+1]:{parts[1]} + {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} - {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} - {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} - {parts[2]} - C";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} - {parts[2]} - C";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3
                    ? mnemonic
                    : $"R[{parts[1]}+1]:{parts[1]} ← R[{parts[1]}+1]:{parts[1]} - {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} ∧ {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} ∧ {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} ∨ {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} ⊕ {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"{parts[1]} ← 0xFF - {parts[1]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"{parts[1]} ← 0x00 - {parts[1]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} ∨ {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[1]} ∧ (0xFF - {parts[2]})";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"{parts[1]} ← {parts[1]} + 1";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"{parts[1]} ← {parts[1]} - 1";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"{parts[1]} ← {parts[1]} ∧ {parts[1]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"{parts[1]} ← {parts[1]} ⊕ {parts[1]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"{parts[1]} ← 0xFF";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"R1:R0 ← {parts[1]} × {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"R1:R0 ← {parts[1]} × {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"R1:R0 ← {parts[1]} × {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"R1:R0 ← ({parts[1]} × {parts[2]}) << 1";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"R1:R0 ← ({parts[1]} × {parts[2]}) << 1";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"Encrypt/Decrypt(R15:R0, {parts[1]})";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["IJMP"] = new AvrInstructionInfo
        {
            Mnemonic = "IJMP",
            Operands = "—",
            Description = "Indirect Jump (Z)",
            OperationFunc = _ => "PC ← Z",
            FlagsAffected = "None"
        },
        ["EIJMP"] = new AvrInstructionInfo
        {
            Mnemonic = "EIJMP",
            Operands = "—",
            Description = "Extended Indirect Jump",
            OperationFunc = _ => "PC ← EIND:Z",
            FlagsAffected = "None"
        },
        ["JMP"] = new AvrInstructionInfo
        {
            Mnemonic = "JMP",
            Operands = "k",
            Description = "Jump",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"PC ← {parts[1]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["ICALL"] = new AvrInstructionInfo
        {
            Mnemonic = "ICALL",
            Operands = "—",
            Description = "Indirect Call (Z)",
            OperationFunc = _ => "PC ← Z",
            FlagsAffected = "None"
        },
        ["EICALL"] = new AvrInstructionInfo
        {
            Mnemonic = "EICALL",
            Operands = "—",
            Description = "Extended Indirect Call",
            OperationFunc = _ => "PC ← EIND:Z",
            FlagsAffected = "None"
        },
        ["CALL"] = new AvrInstructionInfo
        {
            Mnemonic = "CALL",
            Operands = "k",
            Description = "Call Subroutine",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"PC ← {parts[1]}";
            },
            FlagsAffected = "None"
        },
        ["RET"] = new AvrInstructionInfo
        {
            Mnemonic = "RET",
            Operands = "—",
            Description = "Subroutine Return",
            OperationFunc = _ => "PC ← STACK",
            FlagsAffected = "None"
        },
        ["RETI"] = new AvrInstructionInfo
        {
            Mnemonic = "RETI",
            Operands = "—",
            Description = "Interrupt Return",
            OperationFunc = _ => "PC ← STACK",
            FlagsAffected = "I"
        },
        ["CPSE"] = new AvrInstructionInfo
        {
            Mnemonic = "CPSE",
            Operands = "Rd, Rr",
            Description = "Compare, Skip if Equal",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} - {parts[2]}";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} - {parts[2]} - C";
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
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} - {parts[2]}";
            },
            FlagsAffected = "Z,C,N,V,S,H"
        },
        ["SBRC"] = new AvrInstructionInfo
        {
            Mnemonic = "SBRC",
            Operands = "Rr, b",
            Description = "Skip if Bit in Register Cleared",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"if (bit({parts[1]}, {parts[2]}) == 0) PC ← PC + 2 or 3";
            },
            FlagsAffected = "None"
        },
        ["SBRS"] = new AvrInstructionInfo
        {
            Mnemonic = "SBRS",
            Operands = "Rr, b",
            Description = "Skip if Bit in Register Set",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"if (bit({parts[1]}, {parts[2]}) == 1) PC ← PC + 2 or 3";
            },
            FlagsAffected = "None"
        },
        ["SBIC"] = new AvrInstructionInfo
        {
            Mnemonic = "SBIC",
            Operands = "A, b",
            Description = "Skip if Bit in I/O Register Cleared",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"if (bit(IO({parts[1]}), {parts[2]}) == 0) PC ← PC + 2 or 3";
            },
            FlagsAffected = "None"
        },
        ["SBIS"] = new AvrInstructionInfo
        {
            Mnemonic = "SBIS",
            Operands = "A, b",
            Description = "Skip if Bit in I/O Register Set",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"if (bit(IO({parts[1]}), {parts[2]}) == 1) PC ← PC + 2 or 3";
            },
            FlagsAffected = "None"
        },
        ["BRBS"] = new AvrInstructionInfo
        {
            Mnemonic = "BRBS",
            Operands = "s, k",
            Description = "Branch if Status Flag Set",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"if (SREG({parts[1]}) == 1) PC ← PC + {parts[2]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRBC"] = new AvrInstructionInfo
        {
            Mnemonic = "BRBC",
            Operands = "s, k",
            Description = "Branch if Status Flag Cleared",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"if (SREG({parts[1]}) == 0) PC ← PC + {parts[2]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BREQ"] = new AvrInstructionInfo
        {
            Mnemonic = "BREQ",
            Operands = "k",
            Description = "Branch if Equal (Z == 1)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (Z == 1) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRNE"] = new AvrInstructionInfo
        {
            Mnemonic = "BRNE",
            Operands = "k",
            Description = "Branch if Not Equal (Z == 0)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (Z == 0) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRCS"] = new AvrInstructionInfo
        {
            Mnemonic = "BRCS",
            Operands = "k",
            Description = "Branch if Carry Set (C == 1)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (C == 1) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRCC"] = new AvrInstructionInfo
        {
            Mnemonic = "BRCC",
            Operands = "k",
            Description = "Branch if Carry Cleared (C == 0)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (C == 0) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRSH"] = new AvrInstructionInfo
        {
            Mnemonic = "BRSH",
            Operands = "k",
            Description = "Branch if Same or Higher (C == 0)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (C == 0) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRLO"] = new AvrInstructionInfo
        {
            Mnemonic = "BRLO",
            Operands = "k",
            Description = "Branch if Lower (C == 1)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (C == 1) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRMI"] = new AvrInstructionInfo
        {
            Mnemonic = "BRMI",
            Operands = "k",
            Description = "Branch if Minus (N == 1)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (N == 1) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRPL"] = new AvrInstructionInfo
        {
            Mnemonic = "BRPL",
            Operands = "k",
            Description = "Branch if Plus (N == 0)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (N == 0) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRGE"] = new AvrInstructionInfo
        {
            Mnemonic = "BRGE",
            Operands = "k",
            Description = "Branch if Greater or Equal (S == 0, Signed)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (S == 0) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRLT"] = new AvrInstructionInfo
        {
            Mnemonic = "BRLT",
            Operands = "k",
            Description = "Branch if Less Than (S == 1, Signed)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (S == 1) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRHS"] = new AvrInstructionInfo
        {
            Mnemonic = "BRHS",
            Operands = "k",
            Description = "Branch if Half Carry Flag Set (H == 1)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (H == 1) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRHC"] = new AvrInstructionInfo
        {
            Mnemonic = "BRHC",
            Operands = "k",
            Description = "Branch if Half Carry Flag Cleared (H == 0)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (H == 0) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRTS"] = new AvrInstructionInfo
        {
            Mnemonic = "BRTS",
            Operands = "k",
            Description = "Branch if T Bit Set (T == 1)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (T == 1) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRTC"] = new AvrInstructionInfo
        {
            Mnemonic = "BRTC",
            Operands = "k",
            Description = "Branch if T Bit Cleared (T == 0)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (T == 0) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRVS"] = new AvrInstructionInfo
        {
            Mnemonic = "BRVS",
            Operands = "k",
            Description = "Branch if Overflow Flag Set (V == 1)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (V == 1) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRVC"] = new AvrInstructionInfo
        {
            Mnemonic = "BRVC",
            Operands = "k",
            Description = "Branch if Overflow Flag Cleared (V == 0)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (V == 0) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRIE"] = new AvrInstructionInfo
        {
            Mnemonic = "BRIE",
            Operands = "k",
            Description = "Branch if Interrupt Enabled (I == 1)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (I == 1) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["BRID"] = new AvrInstructionInfo
        {
            Mnemonic = "BRID",
            Operands = "k",
            Description = "Branch if Interrupt Disabled (I == 0)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 2 ? mnemonic : $"if (I == 0) PC ← PC + {parts[1]} + 1";
            },
            FlagsAffected = "None"
        },
        ["MOV"] = new AvrInstructionInfo
        {
            Mnemonic = "MOV",
            Operands = "Rd, Rr",
            Description = "Copy Register",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[2]}";
            },
            FlagsAffected = "None"
        },
        ["MOVW"] = new AvrInstructionInfo
        {
            Mnemonic = "MOVW",
            Operands = "Rd, Rr",
            Description = "Copy Register Pair",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"R[{parts[1]}+1]:{parts[1]} ← R[{parts[2]}+1]:{parts[2]}";
            },
            FlagsAffected = "None"
        },
        ["LDI"] = new AvrInstructionInfo
        {
            Mnemonic = "LDI",
            Operands = "Rd, K",
            Description = "Load Immediate",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← {parts[2]}";
            },
            FlagsAffected = "None"
        },
        ["LDS"] = new AvrInstructionInfo
        {
            Mnemonic = "LDS",
            Operands = "Rd, k",
            Description = "Load Direct from Data Space",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]} ← DS({parts[2]})";
            },
            FlagsAffected = "None"
        },
        ["LD"] = new AvrInstructionInfo
        {
            Mnemonic = "LD",
            Operands = "Rd, X/Y/Z [with optional + or -]",
            Description = "Load Indirect from Data Space",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;

                var rd = parts[1];
                var addr = parts[2].ToUpperInvariant();

                if (addr.EndsWith("+"))
                    return $"{rd} ← DS({addr.TrimEnd('+')}); {addr.TrimEnd('+')} ← {addr.TrimEnd('+')} + 1";
                if (addr.StartsWith("-"))
                    return $"{addr.TrimStart('-')} ← {addr.TrimStart('-')} - 1; {rd} ← DS({addr.TrimStart('-')})";

                return $"{rd} ← DS({addr})";
            },
            FlagsAffected = "None"
        },
        ["LDD"] = new AvrInstructionInfo
        {
            Mnemonic = "LDD",
            Operands = "Rd, Y/Z+q",
            Description = "Load Indirect with Displacement",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;

                var rd = parts[1];
                var addr = parts[2].ToUpperInvariant();

                // Expect Y+q or Z+q
                if (addr.StartsWith("Y+"))
                    return $"{rd} ← DS(Y + {addr.Substring(2)})";
                if (addr.StartsWith("Z+"))
                    return $"{rd} ← DS(Z + {addr.Substring(2)})";

                return $"{rd} ← DS({addr})";
            },
            FlagsAffected = "None"
        },

        ["STD"] = new AvrInstructionInfo
        {
            Mnemonic = "STD",
            Operands = "Y/Z+q, Rr",
            Description = "Store Indirect with Displacement",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;

                var addr = parts[1].ToUpperInvariant();
                var rr = parts[2];

                if (addr.StartsWith("Y+"))
                    return $"DS(Y + {addr.Substring(2)}) ← {rr}";
                if (addr.StartsWith("Z+"))
                    return $"DS(Z + {addr.Substring(2)}) ← {rr}";

                return $"DS({addr}) ← {rr}";
            },
            FlagsAffected = "None"
        },
        ["ST"] = new AvrInstructionInfo
        {
            Mnemonic = "ST",
            Operands = "X/Y/Z, Rr [with optional + or -]",
            Description = "Store Indirect",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return mnemonic;

                var addr = parts[1].ToUpperInvariant();
                var rr = parts[2];

                if (addr.EndsWith("+"))
                    return $"DS({addr.TrimEnd('+')}) ← {rr}; {addr.TrimEnd('+')} ← {addr.TrimEnd('+')} + 1";
                if (addr.StartsWith("-"))
                    return $"{addr.TrimStart('-')} ← {addr.TrimStart('-')} - 1; DS({addr.TrimStart('-')}) ← {rr}";

                return $"DS({addr}) ← {rr}";
            },
            FlagsAffected = "None"
        },
        ["LPM"] = new AvrInstructionInfo
        {
            Mnemonic = "LPM",
            Operands = "[Rd,] Z[+]",
            Description = "Load from Program Memory",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1)
                    return "R0 ← PS(Z)";
                if (parts.Length == 3 && parts[2].EndsWith("+"))
                    return $"{parts[1]} ← PS(Z); Z ← Z + 1";
                if (parts.Length == 3)
                    return $"{parts[1]} ← PS(Z)";

                return mnemonic;
            },
            FlagsAffected = "None"
        },

        ["ELPM"] = new AvrInstructionInfo
        {
            Mnemonic = "ELPM",
            Operands = "[Rd,] Z[+]",
            Description = "Extended Load from Program Memory (RAMPZ:Z)",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1)
                    return "R0 ← PS(RAMPZ:Z)";
                if (parts.Length == 3 && parts[2].EndsWith("+"))
                    return $"{parts[1]} ← PS(RAMPZ:Z); (RAMPZ:Z) ← (RAMPZ:Z) + 1";
                if (parts.Length == 3)
                    return $"{parts[1]} ← PS(RAMPZ:Z)";

                return mnemonic;
            },
            FlagsAffected = "None"
        },
        ["SPM"] = new AvrInstructionInfo
        {
            Mnemonic = "SPM",
            Operands = "Z or Z+",
            Description = "Store Program Memory",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return "PS(RAMPZ:Z) ← R1:R0";

                var addr = parts[1].ToUpperInvariant();
                if (addr.EndsWith("+"))
                    return "PS(RAMPZ:Z) ← R1:R0; Z ← Z + 2";
                return "PS(RAMPZ:Z) ← R1:R0";
            },
            FlagsAffected = "None"
        },
        ["LSL"] = new AvrInstructionInfo
        {
            Mnemonic = "LSL",
            Operands = "Rd",
            Description = "Logical Shift Left",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                var rd = parts[1];
                return $"{rd} ← {rd} << 1; C ← old bit7";
            },
            FlagsAffected = "Z, N, V, S, C"
        },

        ["LSR"] = new AvrInstructionInfo
        {
            Mnemonic = "LSR",
            Operands = "Rd",
            Description = "Logical Shift Right",
            OperationFunc = mnemonic =>
            {
                var rd = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[1];
                return $"{rd} ← {rd} >> 1; C ← old bit0";
            },
            FlagsAffected = "Z, N, V, S, C"
        },

        ["ROL"] = new AvrInstructionInfo
        {
            Mnemonic = "ROL",
            Operands = "Rd",
            Description = "Rotate Left Through Carry",
            OperationFunc = mnemonic =>
            {
                var rd = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[1];
                return $"{rd} ← ({rd} << 1) + C";
            },
            FlagsAffected = "Z, N, V, S, C"
        },

        ["ROR"] = new AvrInstructionInfo
        {
            Mnemonic = "ROR",
            Operands = "Rd",
            Description = "Rotate Right Through Carry",
            OperationFunc = mnemonic =>
            {
                var rd = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[1];
                return $"{rd} ← (C << 7) + ({rd} >> 1)";
            },
            FlagsAffected = "Z, N, V, S, C"
        },

        ["ASR"] = new AvrInstructionInfo
        {
            Mnemonic = "ASR",
            Operands = "Rd",
            Description = "Arithmetic Shift Right",
            OperationFunc = mnemonic =>
            {
                var rd = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[1];
                return $"{rd} ← ({rd} >> 1) with sign extend; C ← old bit0";
            },
            FlagsAffected = "Z, N, V, S, C"
        },

        ["SWAP"] = new AvrInstructionInfo
        {
            Mnemonic = "SWAP",
            Operands = "Rd",
            Description = "Swap Nibbles",
            OperationFunc = mnemonic =>
            {
                var rd = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[1];
                return $"{rd} ← Swap({rd}[7:4], {rd}[3:0])";
            },
            FlagsAffected = "None"
        },

        ["SBI"] = new AvrInstructionInfo
        {
            Mnemonic = "SBI",
            Operands = "A, b",
            Description = "Set Bit in I/O Register",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"I/O({parts[1]})[{parts[2]}] ← 1";
            },
            FlagsAffected = "None"
        },

        ["CBI"] = new AvrInstructionInfo
        {
            Mnemonic = "CBI",
            Operands = "A, b",
            Description = "Clear Bit in I/O Register",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"I/O({parts[1]})[{parts[2]}] ← 0";
            },
            FlagsAffected = "None"
        },

        ["BST"] = new AvrInstructionInfo
        {
            Mnemonic = "BST",
            Operands = "Rr, b",
            Description = "Bit Store from Register to T",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"T ← {parts[1]}[{parts[2]}]";
            },
            FlagsAffected = "None"
        },

        ["BLD"] = new AvrInstructionInfo
        {
            Mnemonic = "BLD",
            Operands = "Rd, b",
            Description = "Bit Load from T to Register",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', ',', '\t'], StringSplitOptions.RemoveEmptyEntries);
                return parts.Length < 3 ? mnemonic : $"{parts[1]}[{parts[2]}] ← T";
            },
            FlagsAffected = "None"
        },

        ["BSET"] = new AvrInstructionInfo
        {
            Mnemonic = "BSET",
            Operands = "s",
            Description = "Flag Set",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                var s = parts[1];
                return $"SREG({s}) ← 1";
            },
            FlagsAffected = "Status Flag Set"
        },
        ["BCLR"] = new AvrInstructionInfo
        {
            Mnemonic = "BCLR",
            Operands = "s",
            Description = "Flag Clear",
            OperationFunc = mnemonic =>
            {
                var parts = mnemonic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return mnemonic;
                var s = parts[1];
                return $"SREG({s}) ← 0";
            },
            FlagsAffected = "Status Flag Cleared"
        },
        ["SEC"] = new AvrInstructionInfo
        {
            Mnemonic = "SEC",
            Operands = "",
            Description = "Set Carry Flag",
            OperationFunc = _ => "C ← 1",
            FlagsAffected = "C"
        },
        ["CLC"] = new AvrInstructionInfo
        {
            Mnemonic = "CLC",
            Operands = "",
            Description = "Clear Carry Flag",
            OperationFunc = _ => "C ← 0",
            FlagsAffected = "C"
        },
        ["SEN"] = new AvrInstructionInfo
        {
            Mnemonic = "SEN",
            Operands = "",
            Description = "Set Negative Flag",
            OperationFunc = _ => "N ← 1",
            FlagsAffected = "N"
        },
        ["CLN"] = new AvrInstructionInfo
        {
            Mnemonic = "CLN",
            Operands = "",
            Description = "Clear Negative Flag",
            OperationFunc = _ => "N ← 0",
            FlagsAffected = "N"
        },
        ["SEZ"] = new AvrInstructionInfo
        {
            Mnemonic = "SEZ",
            Operands = "",
            Description = "Set Zero Flag",
            OperationFunc = _ => "Z ← 1",
            FlagsAffected = "Z"
        },
        ["CLZ"] = new AvrInstructionInfo
        {
            Mnemonic = "CLZ",
            Operands = "",
            Description = "Clear Zero Flag",
            OperationFunc = _ => "Z ← 0",
            FlagsAffected = "Z"
        },
        ["CLZ"] = new AvrInstructionInfo
        {
            Mnemonic = "CLZ",
            Operands = "",
            Description = "Clear Zero Flag",
            OperationFunc = _ => "Z ← 0",
            FlagsAffected = "Z"
        },

        ["SEI"] = new AvrInstructionInfo
        {
            Mnemonic = "SEI",
            Operands = "",
            Description = "Global Interrupt Enable",
            OperationFunc = _ => "I ← 1",
            FlagsAffected = "I"
        },

        ["CLI"] = new AvrInstructionInfo
        {
            Mnemonic = "CLI",
            Operands = "",
            Description = "Global Interrupt Disable",
            OperationFunc = _ => "I ← 0",
            FlagsAffected = "I"
        },

        ["SES"] = new AvrInstructionInfo
        {
            Mnemonic = "SES",
            Operands = "",
            Description = "Set Sign Bit",
            OperationFunc = _ => "S ← 1",
            FlagsAffected = "S"
        },

        ["CLS"] = new AvrInstructionInfo
        {
            Mnemonic = "CLS",
            Operands = "",
            Description = "Clear Sign Bit",
            OperationFunc = _ => "S ← 0",
            FlagsAffected = "S"
        },

        ["SEV"] = new AvrInstructionInfo
        {
            Mnemonic = "SEV",
            Operands = "",
            Description = "Set Two’s Complement Overflow",
            OperationFunc = _ => "V ← 1",
            FlagsAffected = "V"
        },

        ["CLV"] = new AvrInstructionInfo
        {
            Mnemonic = "CLV",
            Operands = "",
            Description = "Clear Two’s Complement Overflow",
            OperationFunc = _ => "V ← 0",
            FlagsAffected = "V"
        },

        ["SET"] = new AvrInstructionInfo
        {
            Mnemonic = "SET",
            Operands = "",
            Description = "Set T in SREG",
            OperationFunc = _ => "T ← 1",
            FlagsAffected = "T"
        },

        ["CLT"] = new AvrInstructionInfo
        {
            Mnemonic = "CLT",
            Operands = "",
            Description = "Clear T in SREG",
            OperationFunc = _ => "T ← 0",
            FlagsAffected = "T"
        },

        ["SEH"] = new AvrInstructionInfo
        {
            Mnemonic = "SEH",
            Operands = "",
            Description = "Set Half Carry Flag in SREG",
            OperationFunc = _ => "H ← 1",
            FlagsAffected = "H"
        },

        ["CLH"] = new AvrInstructionInfo
        {
            Mnemonic = "CLH",
            Operands = "",
            Description = "Clear Half Carry Flag in SREG",
            OperationFunc = _ => "H ← 0",
            FlagsAffected = "H"
        },
        
        ["BREAK"] = new AvrInstructionInfo
        {
            Mnemonic = "BREAK",
            Operands = "",
            Description = "Break - used for debugging; halts program execution until resumed by the debug interface.",
            OperationFunc = _ => "Invoke debug break; program halted until resumed",
            FlagsAffected = "None"
        },

        ["NOP"] = new AvrInstructionInfo
        {
            Mnemonic = "NOP",
            Operands = "",
            Description = "No Operation",
            OperationFunc = _ => "No operation (do nothing for one clock cycle)",
            FlagsAffected = "None"
        },

        ["SLEEP"] = new AvrInstructionInfo
        {
            Mnemonic = "SLEEP",
            Operands = "",
            Description = "Enter Sleep Mode - execution halted until interrupt or reset",
            OperationFunc = _ => "Enter sleep mode (depends on MCU sleep configuration)",
            FlagsAffected = "None"
        },

        ["WDR"] = new AvrInstructionInfo
        {
            Mnemonic = "WDR",
            Operands = "",
            Description = "Watchdog Reset - resets the Watchdog Timer",
            OperationFunc = _ => "Reset watchdog timer (prevent system reset)",
            FlagsAffected = "None"
        },
    };
}

public record LssInstruction
{
    public string Section { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Opcode { get; init; } = string.Empty;
    public string Mnemonic { get; init; } = string.Empty;
    public string FlagsAffected { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;

    public override string ToString()
    {
        return
            $"{Section,-8} {Address,-6} {Opcode,-10} {Mnemonic,-35} | {FlagsAffected,-10} | {Symbol,-30} | {Comment}";
    }
}

public class LssParser
{
    private static readonly Regex SectionRegex = new(@"\.(cseg|dseg|eseg)", RegexOptions.IgnoreCase);
    // Updated regex to allow for optional label (e.g., "set6:") before mnemonic
    private static readonly Regex CodeLineRegex =
        new(@"^\s*([0-9A-Fa-f]{6})\s+([0-9A-Fa-f ]{4,10})\s+(?:(\w+):\s*)?([a-zA-Z].*?)(?:\s*;\s*(.*))?$",
            RegexOptions.Compiled);


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

            var match = CodeLineRegex.Match(line);if (match.Success)
            {
                var address = match.Groups[1].Value.Trim();
                var opcode = match.Groups[2].Value.Trim();
                var label = match.Groups[3].Value.Trim();  // optional, may be empty
                var mnemonic = match.Groups[4].Value.Trim();
                var comment = match.Groups[5].Value.Trim();

                // Extract mnemonic key (first token only)
                var mnemonicKey = mnemonic.Split(' ')[0].ToUpperInvariant();

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
                    Label = label,
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
                ConvertLssToExcel(inputPath, customOutputDirectory);
            }
            else
            {
                throw new InvalidOperationException("Not an .lss file: " + inputPath);
            }
        }
        else if (Directory.Exists(inputPath))
        {
            var lssFiles = Directory.GetFiles(inputPath, "*.lss", SearchOption.TopDirectoryOnly);
            if (lssFiles.Length == 0)
                throw new InvalidOperationException("No .lss files found in directory: " + inputPath);

            foreach (var lssFile in lssFiles)
            {
                progress?.Invoke($"Converting file: {Path.GetFileName(lssFile)}");
                ConvertLssToExcel(lssFile, customOutputDirectory);
            }
        }
        else
        {
            throw new FileNotFoundException("Path not found: " + inputPath);
        }
    }

    private static void ConvertLssToExcel(string filePath, string? customOutputDirectory)
    {
        var parser = new LssParser();
        var instructions = parser.Parse(filePath);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("LSS");

        // Header row
        SetAndStylizeHeaderCell(ws.Cell(1, 1), "Section");
        SetAndStylizeHeaderCell(ws.Cell(1, 2), "Address");
        SetAndStylizeHeaderCell(ws.Cell(1, 3), "Label");
        SetAndStylizeHeaderCell(ws.Cell(1, 4), "Opcode");
        SetAndStylizeHeaderCell(ws.Cell(1, 5), "Mnemonic");
        SetAndStylizeHeaderCell(ws.Cell(1, 6), "Flags Affected");
        SetAndStylizeHeaderCell(ws.Cell(1, 7), "Symbol");
        SetAndStylizeHeaderCell(ws.Cell(1, 8), "Comment");

        int row = 2;
        foreach (var instr in instructions)
        {
            SetAndStylizeCell(ws.Cell(row, 1), instr.Section);
            SetAndStylizeCell(ws.Cell(row, 2), instr.Address);
            SetAndStylizeCell(ws.Cell(row, 3), instr.Label);
            SetAndStylizeCell(ws.Cell(row, 4), instr.Opcode);
            SetAndStylizeCell(ws.Cell(row, 5), instr.Mnemonic);
            SetAndStylizeCell(ws.Cell(row, 6), instr.FlagsAffected);
            SetAndStylizeCell(ws.Cell(row, 7), instr.Symbol);
            SetAndStylizeCell(ws.Cell(row, 8), instr.Comment);
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
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontColor = XLColor.White;
        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    private static void SetAndStylizeCell(IXLCell cell, XLCellValue value)
    {
        cell.Value = value;
        cell.Style.Font.FontName = "Cascadia Code";
        cell.Style.Font.FontSize = 9;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    } }