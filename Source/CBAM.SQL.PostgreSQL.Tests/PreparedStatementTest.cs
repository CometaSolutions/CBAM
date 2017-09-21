﻿/*
 * Copyright 2017 Stanislav Muhametsin. All rights Reserved.
 *
 * Licensed  under the  Apache License,  Version 2.0  (the "License");
 * you may not use  this file  except in  compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed  under the  License is distributed on an "AS IS" BASIS,
 * WITHOUT  WARRANTIES OR CONDITIONS  OF ANY KIND, either  express  or
 * implied.
 *
 * See the License for the specific language governing permissions and
 * limitations under the License. 
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CBAM.SQL.PostgreSQL.Tests
{
   [TestClass]
   public class PreparedStatementTest : AbstractPostgreSQLTest
   {
      [
         DataTestMethod,
         DataRow( DEFAULT_CONFIG_FILE_LOCATION ),
         Timeout( DEFAULT_TIMEOUT )
         ]
      public async Task TestPreparedStatement_Integers( String connectionConfigFileLocation )
      {
         const Int32 FIRST = 1;
         const Int32 SECOND = 2;
         const Int32 THIRD = 3;
         var pool = GetPool( GetConnectionCreationInfo( connectionConfigFileLocation ) );

         var tuple = await pool.UseResourceAsync( async conn =>
         {
            var stmt = conn.CreateStatementBuilder( "SELECT * FROM( VALUES( ? ), ( ? ), ( ? ) ) AS tmp" );
            stmt.SetParameterInt32( 0, FIRST );
            stmt.SetParameterInt32( 1, SECOND );
            stmt.SetParameterInt32( 2, THIRD );

            var iArgs = conn.PrepareStatementForExecution( stmt );
            Int64? tkn;
            Assert.IsTrue( ( tkn = await iArgs.MoveNextAsync() ).HasValue );
            var seenFirst = await iArgs.GetDataRow( tkn ).GetValueAsync<Int32>( 0 );

            Assert.IsTrue( ( tkn = await iArgs.MoveNextAsync() ).HasValue );
            var seenSecond = await iArgs.GetDataRow( tkn ).GetValueAsync<Int32>( 0 );

            Assert.IsTrue( ( tkn = await iArgs.MoveNextAsync() ).HasValue );
            var seenThird = await iArgs.GetDataRow( tkn ).GetValueAsync<Int32>( 0 );

            Assert.IsFalse( ( tkn = await iArgs.MoveNextAsync() ).HasValue );


            await AssertThatConnectionIsStillUseable( conn, iArgs );

            return (seenFirst, seenSecond, seenThird);
         } );

         Assert.AreEqual( FIRST, tuple.Item1 );
         Assert.AreEqual( SECOND, tuple.Item2 );
         Assert.AreEqual( THIRD, tuple.Item3 );
      }

      [
         DataTestMethod,
         DataRow( DEFAULT_CONFIG_FILE_LOCATION ),
         Timeout( DEFAULT_TIMEOUT )
         ]
      public async Task TestPreparedStatement_Strings( String connectionConfigFileLocation )
      {
         const String FIRST = "first";
         const String SECOND = "second";
         const String THIRD = "third";
         var pool = GetPool( GetConnectionCreationInfo( connectionConfigFileLocation ) );

         var tuple = await pool.UseResourceAsync( async conn =>
         {
            var stmt = conn.CreateStatementBuilder( "SELECT * FROM ( VALUES( ? ), ( ? ), ( ? ) ) AS tmp" );
            stmt.SetParameterString( 0, FIRST );
            stmt.SetParameterString( 1, SECOND );
            stmt.SetParameterString( 2, THIRD );

            var iArgs = conn.PrepareStatementForExecution( stmt );
            Int64? tkn;
            Assert.IsTrue( ( tkn = await iArgs.MoveNextAsync() ).HasValue );
            var seenFirst = await iArgs.GetDataRow( tkn ).GetValueAsync<String>( 0 );

            Assert.IsTrue( ( tkn = await iArgs.MoveNextAsync() ).HasValue );
            var seenSecond = await iArgs.GetDataRow( tkn ).GetValueAsync<String>( 0 );

            Assert.IsTrue( ( tkn = await iArgs.MoveNextAsync() ).HasValue );
            var seenThird = await iArgs.GetDataRow( tkn ).GetValueAsync<String>( 0 );

            Assert.IsFalse( ( tkn = await iArgs.MoveNextAsync() ).HasValue );

            await AssertThatConnectionIsStillUseable( conn, iArgs );

            return (seenFirst, seenSecond, seenThird);
         } );

         Assert.AreEqual( FIRST, tuple.Item1 );
         Assert.AreEqual( SECOND, tuple.Item2 );
         Assert.AreEqual( THIRD, tuple.Item3 );
      }


      [DataTestMethod,
         DataRow(
         DEFAULT_CONFIG_FILE_LOCATION,
         typeof( TextArrayGenerator )
         ),
         Timeout( DEFAULT_TIMEOUT )
         ]
      public async Task TestPreparedStatement_Arrays_TestReceive(
         String connectionConfigFileLocation,
         Type arrayGenerator
         )
      {
         var generator = (SimpleArrayDataGenerator) Activator.CreateInstance( arrayGenerator );

         await TestWithAndWithoutBinaryReceive( connectionConfigFileLocation, async conn =>
         {
            var stmt = conn.VendorFunctionality.CreateStatementBuilder( "SELECT ?" );
            foreach ( var arrayInfo in generator.GenerateArrays() )
            {
               var array = arrayInfo.Array;
               stmt.SetParameterObjectWithType( 0, array, array.GetType().GetElementType().MakeArrayType() );
               ValidateArrays( array, await conn.GetFirstOrDefaultAsync<Array>( stmt ) );
            }
         } );
      }

      [DataTestMethod,
         DataRow(
         DEFAULT_CONFIG_FILE_LOCATION,
         typeof( TextArrayGenerator )
         ),
         Timeout( DEFAULT_TIMEOUT )
         ]
      public async Task TestPreparedStatement_Arrays_TestSend(
         String connectionConfigFileLocation,
         Type arrayGenerator
         )
      {
         var generator = (SimpleArrayDataGenerator) Activator.CreateInstance( arrayGenerator );
         await TestWithAndWithoutBinarySend( connectionConfigFileLocation, async conn =>
         {
            var stmt = conn.VendorFunctionality.CreateStatementBuilder( "SELECT ?" );
            foreach ( var arrayInfo in generator.GenerateArrays() )
            {
               var array = arrayInfo.Array;
               stmt.SetParameterObjectWithType( 0, array, array.GetType().GetElementType().MakeArrayType() );
               ValidateArrays( array, await conn.GetFirstOrDefaultAsync<Array>( stmt ) );
            }
         } );
      }

      [DataTestMethod,
         DataRow(
         DEFAULT_CONFIG_FILE_LOCATION
         ),
      Timeout( DEFAULT_TIMEOUT )
      ]
      public async Task TestByteA(
         String connectionConfigFileLocation
         )
      {
         var pool = GetPool( GetConnectionCreationInfo( connectionConfigFileLocation ) );

         var bytez = new Byte[256];
         UtilPack.Cryptography.Digest.DigestBasedRandomGenerator.CreateAndSeedWithDefaultLogic( new UtilPack.Cryptography.Digest.SHA512() ).NextBytes( bytez );

         await pool.UseResourceAsync( async conn =>
         {
            var stmt = conn.CreateStatementBuilder( "SELECT * FROM( VALUES( ? ) ) AS tmp" );
            stmt.SetParameterObject<Byte[]>( 0, bytez );
            await conn.ExecuteAndIgnoreResults( stmt );
         } );
      }
   }
}
