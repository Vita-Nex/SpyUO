using System;
using System.IO;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes ultima class writer.
	/// </summary>
	public class UltimaClassWriter : StreamWriter
	{
		#region Properties
		private int _Indent;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaClassWriter.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		public UltimaClassWriter( Stream stream ) : base( stream )
		{
			_Indent = 0;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Writes using to stream.
		/// </summary>
		/// <param name="name">Namespace name.</param>
		public void WriteUsing( string name )
		{
			WriteLine( "using {0};", name );
		}

		/// <summary>
		/// Writes namespace to stream.
		/// </summary>
		/// <param name="name">Namespace name.</param>
		public void BeginNamespace( string name )
		{
			WriteLine( "namespace {0}", name );
			WriteLine( "{" );
			_Indent = 1;
		}

		/// <summary>
		/// Ends namespace.
		/// </summary>
		public void EndNamespace()
		{
			WriteLine( "}" );
			_Indent = 0;
		}

		/// <summary>
		/// Writes class definition to stream.
		/// </summary>
		/// <param name="name">Class name.</param>
		/// <param name="implementations">Class implementations.</param>
		public void BeginClass( string name, string implementations = null )
		{
			if ( !String.IsNullOrEmpty( implementations ) )
				WriteLineWithIndent( "public class {0} : {1}", name, implementations );
			else
				WriteLineWithIndent( "public class {0}", name );

			WriteLineWithIndent( "{" );
			_Indent += 1;
		}

		/// <summary>
		/// Ends class.
		/// </summary>
		public void EndClass()
		{
			_Indent -= 1;
			WriteLineWithIndent( "}" );
		}

		/// <summary>
		/// Overrides property.
		/// </summary>
		/// <param name="access">Property access.</param>
		/// <param name="type">Property type.</param>
		/// <param name="name">Property name.</param>
		/// <param name="value">Property value.</param>
		/// <param name="comment">Comment.</param>
		public void OverrideProperty( string access, string type, string name, string value, string comment = null )
		{
			if ( comment != null )
				WriteLineWithIndent( "{0} override {1} {2} {{ get {{ return {3}; }} }} // {4}", access, type, name, value, comment );
			else
				WriteLineWithIndent( "{0} override {1} {2} {{ get {{ return {3}; }} }}", access, type, name, value );
		}

		/// <summary>
		/// Writes constructor to stream.
		/// </summary>
		/// <param name="access">Cosntrucor access.</param>
		/// <param name="name">Constructor name.</param>
		/// <param name="parameters">Constructor parameters.</param>
		/// <param name="baseParameters">Base parameters.</param>
		public void BeginConstructor( string access, string name, string parameters = null, string baseParameters = null )
		{
			if ( parameters != null )
			{
				if ( baseParameters != null )
				{
					if (	baseParameters.Length > 0 )
						WriteLineWithIndent( "{0} {1}( {2} ) : base( {3} )", access, name, parameters, baseParameters );
					else
						WriteLineWithIndent( "{0} {1}( {2} ) : base()", access, name, parameters );
				}
				else
					WriteLineWithIndent( "{0} {1}( {2} )", access, name, parameters );
			}
			else
			{
				if ( baseParameters != null )
				{
					if ( baseParameters.Length > 0 )
						WriteLineWithIndent( "{0} {1}() : base( {2} )", access, name, baseParameters );
					else
						WriteLineWithIndent( "{0} {1}() : base()", access, name );
				}
				else
					WriteLineWithIndent( "{0} {1}()", access, name );
			}

			WriteLineWithIndent( "{" ); _Indent++;
		}

		/// <summary>
		/// Ends method.
		/// </summary>
		public void EndConstructor()
		{
			_Indent -= 1;
			WriteLineWithIndent( "}" );
		}

		/// <summary>
		/// Writes method to stream.
		/// </summary>
		/// <param name="access">Method access.</param>
		/// <param name="returnType">Method return type.</param>
		/// <param name="name">Method name.</param>
		/// <param name="parameters">Method parameters.</param>
		public void BeginMethod( string access, string returnType, string name, string parameters )
		{
			WriteLineWithIndent( "{0} {1} {2}( {3} )", access, returnType, name, parameters );
			WriteLineWithIndent( "{" ); _Indent++;
		}

		/// <summary>
		/// Overrides method.
		/// </summary>
		/// <param name="access">Method access.</param>
		/// <param name="returnType">Method return type.</param>
		/// <param name="name">Method name.</param>
		/// <param name="parameters">Method parameters.</param>
		public void BeginOverrideMethod( string access, string returnType, string name, string parameters = null )
		{
			if ( parameters != null )
				WriteLineWithIndent( "{0} override {1} {2}( {3} )", access, returnType, name, parameters );
			else
				WriteLineWithIndent( "{0} override {1} {2}()", access, returnType, name );

			WriteLineWithIndent( "{" ); _Indent++;
		}

		/// <summary>
		/// Ends method.
		/// </summary>
		public void EndMethod()
		{
			_Indent -= 1;
			WriteLineWithIndent( "}" );
		}

		/// <summary>
		/// Writes serialize method.
		/// </summary>
		public void WriteSerialConstructor( string name )
		{
			BeginConstructor( "public", name, "Serial serial", "serial" );
			EndConstructor();
		}

		/// <summary>
		/// Writes serialize method.
		/// </summary>
		public void WriteSerialize()
		{
			BeginOverrideMethod( "public", "void", "Serialize", "GenericWriter writer" );
			WriteLineWithIndent( "base.Serialize( writer );" );
			WriteLine();
			WriteLineWithIndent( "writer.WriteEncodedInt( 0 ); // version" );
			EndMethod();
		}

		/// <summary>
		/// Writes serialize method.
		/// </summary>
		public void WriteDeserialize()
		{
			BeginOverrideMethod( "public", "void", "Deserialize", "GenericReader reader" );
			WriteLineWithIndent( "base.Deserialize( reader );" );
			WriteLine();
			WriteLineWithIndent( "int version = reader.ReadEncodedInt();" );
			EndMethod();
		}

		/// <summary>
		/// Writes string format with indent.
		/// </summary>
		/// <param name="format">String format.</param>
		/// <param name="args">Format parameters.</param>
		public void WriteWithIndent( string format, params object[] args )
		{
			for ( int i = 0; i < _Indent; i++ )
			{
				Write( '\t' );
			}

			if ( args.Length == 0 )
				Write( format );
			else
				Write( String.Format( format, args ) );
		}

		/// <summary>
		/// Writes line string format with indent.
		/// </summary>
		/// <param name="format">String format.</param>
		/// <param name="args">Format parameters.</param>
		public void WriteLineWithIndent( string format, params object[] args )
		{
			for ( int i = 0; i < _Indent; i++ )
			{
				Write( '\t' );
			}

			if ( args.Length == 0 )
				WriteLine( format );
			else
				WriteLine( String.Format( format, args ) );
		}

		/// <summary>
		/// Capitalizes first word letter and removes spaces and special characters.
		/// </summary>
		/// <param name="name">String to make class name from.</param>
		/// <returns>Class name.</returns>
		public static string BuildClassName( string name )
		{
			char[] chars = name.ToCharArray();
			bool makeUpper = true;

			for ( int i = 0; i < chars.Length; i++ )
			{
				char c = chars[ i ];

				if ( makeUpper )
				{
					chars[ i ] = Char.ToUpper( c );
					makeUpper = false;
				}

				if ( Char.IsWhiteSpace( c ) )
					makeUpper = true;
				else if ( Char.IsSymbol( c ) || Char.IsPunctuation( c ) )
					chars[ i ] = ' ';
			}

			return new string( chars ).Replace( " ", null );
		}
		#endregion
	}
}
