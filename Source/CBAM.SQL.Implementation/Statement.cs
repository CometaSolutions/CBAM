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
using CBAM.SQL.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilPack;


namespace CBAM.SQL.Implementation
{
   public class StatementBuilderInformationImpl<TParameter, TList> : SQLStatementBuilderInformation
      where TParameter : StatementParameter
      where TList : class,
#if NET40
      IList<TParameter[]>
#else
      IReadOnlyList<TParameter[]>
#endif
   {
      protected readonly TList _batchParameters;
      protected readonly TParameter[] _currentParameters;

      public StatementBuilderInformationImpl(
         String sql,
         Int32 parameterCount,
         TParameter[] currentParameters,
         TList batchParams
         )
      {
         this.SQL = ArgumentValidator.ValidateNotEmpty( nameof( sql ), sql );
         this.SQLParameterCount = parameterCount;
         this._currentParameters = currentParameters ?? Empty<TParameter>.Array;
         this._batchParameters = ArgumentValidator.ValidateNotNull( nameof( batchParams ), batchParams );
      }

      public Int32 SQLParameterCount { get; }

      public String SQL { get; }

      public Int32 BatchParameterCount => this._batchParameters.Count;

      public StatementParameter GetParameterInfo( Int32 parameterIndex )
      {
         return this._currentParameters.CheckArrayIndexAndReturnOrThrow( parameterIndex, nameof( parameterIndex ) )[parameterIndex];
      }

      public StatementParameter GetBatchParameterInfo( Int32 batchIndex, Int32 parameterIndex )
      {
         return this._batchParameters
            .CheckListIndexAndReturnOrThrow( batchIndex, nameof( batchIndex ) )[batchIndex]
            .CheckArrayIndexAndReturnOrThrow( parameterIndex, nameof( parameterIndex ) )[parameterIndex];
      }
   }

   public abstract class StatementBuilderImpl<TParameter> : StatementBuilderInformationImpl<TParameter, List<TParameter[]>>, SQLStatementBuilder
      where TParameter : StatementParameter
   {

      public StatementBuilderImpl(
         SQLStatementBuilderInformation information,
         TParameter[] currentParams,
         List<TParameter[]> batchParams
         ) : base( ArgumentValidator.ValidateNotNull( nameof( information ), information ).SQL, information.SQLParameterCount, currentParams, batchParams )
      {
         this.StatementBuilderInformation = information;
      }

      public SQLStatementBuilderInformation StatementBuilderInformation { get; }

      public void AddBatch()
      {
         Int32 idx;
         if ( ( idx = Array.FindIndex( this._currentParameters, p => p == null ) ) >= 0 )
         {
            throw new InvalidOperationException( $"The parameter at index {idx} has not been set." );
         }

         //if ( this._batchParameters.Count > 0 )
         //{
         //   // Must verify batch parameters
         //   var prevRow = this._batchParameters[this._batchParameters.Count - 1];
         //   for ( var i = 0; i < this._currentParameters.Length; ++i )
         //   {
         //      var exc = this.VerifyBatchParameters( prevRow[i], this._currentParameters[i] );
         //      if ( exc != null )
         //      {
         //         throw exc;
         //      }
         //   }
         //}

         this._batchParameters.Add( this._currentParameters.CreateArrayCopy() );
         Array.Clear( this._currentParameters, 0, this._currentParameters.Length );
      }

      public void SetParameterObjectWithType( Int32 parameterIndex, Object value, Type clrType )
      {
         if ( clrType == null && value == null )
         {
            throw new ArgumentNullException( $"Both {nameof( value )} and {nameof( clrType )} were null." );
         }
         this._currentParameters[parameterIndex] = this.CreateStatementParameter( parameterIndex, value, clrType );
      }

      protected abstract TParameter CreateStatementParameter( Int32 parameterIndex, Object value, Type clrType );

      //protected abstract SQLException VerifyBatchParameters( TParameter previous, TParameter toBeAdded );

   }

   public class StatementParameterImpl : StatementParameter
   {
      public StatementParameterImpl(
         Type cilType,
         Object value
         )
      {
         this.ParameterCILType = ArgumentValidator.ValidateNotNull( nameof( cilType ), cilType );
         this.ParameterValue = value;
      }

      public Type ParameterCILType { get; }

      public Object ParameterValue { get; }

      //public Boolean Equals( StatementParameter other )
      //{
      //   return ReferenceEquals( this, other )
      //      || ( other != null
      //      && Equals( this.ParameterCILType, other.ParameterCILType )
      //      && Equals( this.ParameterValue, other.ParameterValue )
      //      );
      //}
   }

   // TODO move these to UtilPack
   internal static class CBAMExtensions
   {

      public static Boolean CheckArrayIndex( this Array array, Int32 index )
      {
         return array != null && index >= 0 && index < array.Length;
      }

      public static void CheckArrayIndexOrThrow( this Array array, Int32 index, String indexParameterName = null )
      {
         if ( !array.CheckArrayIndex( index ) )
         {
            throw new ArgumentException( String.IsNullOrEmpty( indexParameterName ) ? "array index" : indexParameterName );
         }
      }

      public static T[] CheckArrayIndexAndReturnOrThrow<T>( this T[] array, Int32 index, String indexParameterName = null )
      {
         array.CheckArrayIndexOrThrow( index, indexParameterName );
         return array;
      }

      // TODO Collections.Generic.IList<T> does not extend Collections.List...

      public static Boolean CheckListIndex<T>( this
#if NET40
      IList<T>
#else
      IReadOnlyList<T>
#endif
         list, Int32 index )
      {
         return list != null && index >= 0 && index < list.Count;
      }

      public static void CheckListIndexOrThrow<T>( this
#if NET40
      IList<T>
#else
      IReadOnlyList<T>
#endif
         list, Int32 index, String indexParameterName = null )
      {
         if ( !list.CheckListIndex( index ) )
         {
            throw new ArgumentException( String.IsNullOrEmpty( indexParameterName ) ? "list index" : indexParameterName );
         }
      }


      public static
#if NET40
      IList<T[]>
#else
      IReadOnlyList<T[]>
#endif
         CheckListIndexAndReturnOrThrow<T>( this
#if NET40
      IList<T[]>
#else
      IReadOnlyList<T[]>
#endif
         list, Int32 index, String indexParameterName = null )
      {
         list.CheckListIndexOrThrow( index, indexParameterName );
         return list;
      }
   }
}


