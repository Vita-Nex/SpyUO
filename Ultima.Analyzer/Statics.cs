using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ultima.Analyzer
{
	/// <summary>
	/// Static stuff.
	/// </summary>
	public static class Statics
	{
		#region SpyUO
		#region Enhanced Recieve Signature
		private static byte[] _EnhancedRecieveSignature = new byte[]
		{
			0x83, 0xEC, 0x1C,
			0x53,
			0x8B, 0x5C, 0x24, 0x24,
			0x8B, 0x43, 0x14,
			0x55,
			0x56,
			0x8B, 0x70, 0x30,
			0x2B, 0x70, 0x34
		};

		/// <summary>
		/// SpyUO recieve key signature for Stygian Abyss client.
		/// <code>
		/// Address     Hex dump        Command
		/// 005D4350    83EC 1C         SUB ESP,1C
		/// 005D4353    53              PUSH EBX
		/// 005D4354    8B5C24 24       MOV EBX,DWORD PTR SS:[ARG.1]
		/// 005D4358    8B43 14         MOV EAX,DWORD PTR DS:[EBX+14]
		/// 005D435B    55              PUSH EBP
		/// 005D435C    56              PUSH ESI
		/// 005D435D    8B70 30         MOV ESI,DWORD PTR DS:[EAX+30]
		/// 005D4360    2B70 34         SUB ESI,DWORD PTR DS:[EAX+34]
		/// </code>
		/// </summary>
		public static byte[] EnhancedRecieveSignature { get { return _EnhancedRecieveSignature; } }
		#endregion

		#region Enhanced Send Signature
		private static byte[] _EnhancedSendSignature = new byte[]
		{
			0x8B, 0x5C, 0x24, 0x2C,
			0x8B, 0x4C, 0x24, 0x28,
			0x83, 0xEC, 0x08,
			0x85, 0xDB,
			0x8B, 0xC4,
			0x89, 0x08,
			0x89, 0x58, 0x4
		};

		/// <summary>
		/// SpyUO send key signature for Stygian Abyss client.
		/// <code>
		/// Address     Hex dump        Command
		/// 005D341F    8B5C24 2C       MOV EBX,DWORD PTR SS:[ARG.5]
		/// 005D3423    8B4C24 28       MOV ECX,DWORD PTR SS:[ARG.4]
		/// 005D3427    83EC 08         SUB ESP,8
		/// 005D342A    85DB            TEST EBX,EBX
		/// 005D342C    8BC4            MOV EAX,ESP
		/// 005D342E    8908            MOV DWORD PTR DS:[EAX],ECX
		/// 005D3430    8958 04         MOV DWORD PTR DS:[EAX+4],EBX
		/// </code>
		/// </summary>
		public static byte[] EnhancedSendSignature { get { return _EnhancedSendSignature; } }
		#endregion

		#region Recieve Signature
		private static byte[] _RecieveSignature = new byte[]
		{
			0x56,
			0x8B, 0xF1,
			0x83, 0xBE, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC,
			0x0F, 0x84, 0xCC, 0xCC, 0xCC, 0xCC,
			0x8B, 0x44, 0x24, 0x08
		};

		/// <summary>
		/// SpyUO recieve key signature for 2D client.
		/// <code>
		/// CPU Disasm
		/// Address   Hex dump                      Command
		/// 00462C60  /$  56                        PUSH ESI
		/// 00462C61  |.  8BF1                      MOV ESI,ECX
		/// 00462C63  |.  83BE E0000A00 00          CMP DWORD PTR DS:[ESI+0A00E0],0
		/// 00462C6A  |.  0F84 FC000000             JE 00462D6C
		/// 00462C70  |.  8B4424 08                 MOV EAX,DWORD PTR SS:[ARG.1]
		/// 00462C74  |.  8038 33                   CMP BYTE PTR DS:[EAX],33
		/// 00462C77  |.  0F85 E5000000             JNE 00462D62
		/// 00462C7D  |.  8078 01 00                CMP BYTE PTR DS:[EAX+1],0
		/// </code>
		/// </summary>
		public static byte[] RecieveSignature { get { return _RecieveSignature; } }
		#endregion

		#region Send Signature
		private static byte[] _SendSignature = new byte[]
		{
			0x56,
			0x8B, 0xF1,
			0x8B, 0x86, 0xC8, 0x00, 0x0A, 0x00,
			0x57,
			0x8B, 0x7C, 0x24, 0x10
		};

		/// <summary>
		/// SpyUO send key signature for 2D client.
		/// <code>
		/// Address     Hex dump        Command
		/// 0045F970    56              PUSH ESI
		/// 0045F971    8BF1            MOV ESI,ECX
		/// 0045F973    8B86 C8000A00   MOV EAX,DWORD PTR DS:[ESI+0A00C8]
		/// 0045F979    57              PUSH EDI
		/// 0045F97A    8B7C24 10       MOV EDI,DWORD PTR SS:[ARG.2]
		/// </code>
		/// </summary>
		public static byte[] SendSignature { get { return _SendSignature; } }
		#endregion

		#region Debug Protection Signature
		private static byte[] _DebugProtectionSignature1 = new byte[]
		{
			0x53,
			0x56,
			0x33, 0xF6,
			0x3B, 0xC6,
			0x57,
			0x74, 0xCC,
			0x39, 0x35, 0xCC, 0xCC, 0xCC, 0xCC,
			0x75, 0xCC,
			0xFF, 0xD0,
			0x85, 0xC0
		};

		/// <summary>
		/// SpyUO send key signature for 2D client.
		/// <code>
		/// Address   Hex dump                   Command
		/// 0059B51F  |.  53                     PUSH EBX
		/// 0059B520  |.  56                     PUSH ESI
		/// 0059B521  |.  33F6                   XOR ESI,ESI
		/// 0059B523  |.  3BC6                   CMP EAX,ESI
		/// 0059B525  |.  57                     PUSH EDI
		/// 0059B526  |.  74 19                  JE SHORT 0059B541
		/// 0059B528  |.  3935 1083A700          CMP DWORD PTR DS:[0A78310],ESI
		/// 0059B52E  |.  75 11                  JNE SHORT 0059B541
		/// 0059B530      FFD0                   CALL EAX
		/// 0059B532  |.  85C0                   TEST EAX,EAX
		/// </code>
		/// </summary>
		public static byte[] DebugProtectionSignature1 { get { return _DebugProtectionSignature1; } }

		private static byte[] _DebugProtectionSignature2 = new byte[]
		{
			0x8D, 0xBC, 0x24, 0x68, 0x02, 0x00, 0x00,
			0xF3, 0xA5,
			0x8B, 0x4C, 0x24, 0x14,
			0x3B, 0x4C, 0x24, 0x20,
			0x75, 0x1A,
			0x8B, 0x54, 0x24, 0x30
		};

		/// <summary>
		/// Debug protection signature 2.
		/// <code>
		/// Address   Hex dump                 Command
		/// 0059B671  |.  8DBC24 68020000      |LEA EDI,[ESP+268]
		/// 0059B678  |.  F3:A5                |REP MOVS DWORD PTR ES:[EDI],DWORD PTR D
		/// 0059B67A  |>  8B4C24 14            |MOV ECX,DWORD PTR SS:[ESP+14]
		/// 0059B67E  |.  3B4C24 20            |CMP ECX,DWORD PTR SS:[ESP+20]
		/// 0059B682  |.  75 1A                |JNE SHORT 0059B69E
		/// 0059B684  |.  8B5424 30            |MOV EDX,DWORD PTR SS:[ESP+30]
		/// </code>
		/// </summary>
		public static byte[] DebugProtectionSignature2 { get { return _DebugProtectionSignature2; } }
		#endregion
		#endregion

		#region Filename Hash Function
		private static byte[] _FileNameSignature = new byte[]
		{
			0x59, 0x89, 0x44, 0x24, 0x10, 0x89, 0x54, 0x24, 0x14, 0xEB
		};

		/// <summary>
		/// File name hash function signature.
		/// <code>
		/// Address     Hex dump        Command
		/// 008DF812    59              POP ECX
		/// 008DF813    894424 10       MOV DWORD PTR SS:[ESP+10],EAX
		/// 008DF817    895424 14       MOV DWORD PTR SS:[ESP+14],EDX
		/// 008DF81B    EB 19           JMP SHORT UOSA.008DF836
		/// </code>
		/// </summary>
		public static byte[] FileNameSignature { get { return _FileNameSignature; } }
		#endregion
	}
}
