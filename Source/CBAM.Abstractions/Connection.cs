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
using CBAM.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilPack;
using UtilPack.AsyncEnumeration;

namespace CBAM.Abstractions
{
   /// <summary>
   /// This is common interface for any kind of connection to the potentially remote resource (e.g. SQL or LDAP server).
   /// </summary>
   /// <typeparam name="TStatement">The type of objects used to manipulate or query remote resource.</typeparam>
   /// <typeparam name="TStatementInformation">The type of objects describing <typeparamref name="TStatement"/>s.</typeparam>
   /// <typeparam name="TStatementCreationArgs">The type of object used to create an instance of <typeparamref name="TStatement"/>.</typeparam>
   /// <typeparam name="TEnumerableItem">The type of object representing the response of manipulation or querying remote resource.</typeparam>
   /// <typeparam name="TVendorFunctionality">The type of object describing vendor-specific information.</typeparam>
   public interface Connection<in TStatement, TStatementInformation, in TStatementCreationArgs, out TEnumerableItem, out TVendorFunctionality> : AsyncEnumerationObservation<TEnumerableItem, TStatementInformation>
      where TVendorFunctionality : ConnectionVendorFunctionality<TStatement, TStatementCreationArgs>
      where TStatement : TStatementInformation
   {
      /// <summary>
      /// Prepares object that will manipulate or query remote resource to be ready for execution.
      /// </summary>
      /// <param name="statement">The statement, which describes querying or manipulating the remote resource.</param>
      /// <returns>Prepared <see cref="AsyncEnumerator{T}"/> to be used to execute the statement.</returns>
      /// <remarks>This method does not execute the <paramref name="statement"/>. Use the methods in <see cref="AsyncEnumerator{T}"/> interface (e.g. <see cref="AsyncEnumerator{T}.MoveNextAsync(CancellationToken)"/>) to actually execute the statement.</remarks>
      AsyncEnumeratorObservable<TEnumerableItem, TStatementInformation> PrepareStatementForExecution( TStatementInformation statement );

      /// <summary>
      /// Gets the <see cref="ConnectionVendorFunctionality{TStatement, TStatementCreationArgs}"/> of this connection.
      /// </summary>
      /// <value>The <see cref="ConnectionVendorFunctionality{TStatement, TStatementCreationArgs}"/> of this connection.</value>
      TVendorFunctionality VendorFunctionality { get; }

      /// <summary>
      /// Gets the current cancellation token for asynchronous operations.
      /// </summary>
      /// <value>The current cancellation token for asynchronous operations.</value>
      /// <remarks>
      /// This will be the <see cref="CancellationToken"/> passed to <see cref="T:UtilPack.ResourcePooling.AsyncResourcePool{TResource}.UseResourceAsync(Func{TResource, Task}, CancellationToken)"/> method.
      /// </remarks>
      /// <seealso cref="T:UtilPack.ResourcePooling.AsyncResourcePool{TConnection}.UseResourceAsync(Func{TConnection, Task}, CancellationToken)"/>
      CancellationToken CurrentCancellationToken { get; }
   }

   /// <summary>
   /// This interface represents vendor-specific functionality that is required by <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/>.
   /// </summary>
   /// <typeparam name="TStatement">The type of statement object used to manipulate or query the remote resource.</typeparam>
   /// <typeparam name="TStatementCreationArgs">The type of parameters used to create statement object.</typeparam>
   public interface ConnectionVendorFunctionality<out TStatement, in TStatementCreationArgs>
   {
      /// <summary>
      /// Creates a modifiable statement object, which can be customized and parametrized as needed in order to manipulate or query remote resource.
      /// </summary>
      /// <param name="creationArgs">The object that contains information that will not be customizable in resulting statement builder.</param>
      /// <returns>Customizable statement object.</returns>
      TStatement CreateStatementBuilder( TStatementCreationArgs creationArgs );
   }
}


/// <summary>
/// This class contains extension methods for types defined in this assembly.
/// </summary>
public static partial class E_CBAM
{
   /// <summary>
   /// This is shortcut method to create a new statement from the <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}.VendorFunctionality"/> of this <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/>.
   /// </summary>
   /// <typeparam name="TStatement">The type of objects used to manipulate or query remote resource.</typeparam>
   /// <typeparam name="TStatementInformation">The type of objects describing <typeparamref name="TStatement"/>s.</typeparam>
   /// <typeparam name="TStatementCreationArgs">The type of object used to create an instance of <typeparamref name="TStatement"/>.</typeparam>
   /// <typeparam name="TEnumerableItem">The type of object representing the response of manipulation or querying remote resource.</typeparam>
   /// <typeparam name="TVendorFunctionality">The type of object describing vendor-specific information.</typeparam>
   /// <param name="connection">This <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/>.</param>
   /// <param name="creationArgs">The statement builder creation parameters.</param>
   /// <returns>A new instance of statement builder with given <paramref name="creationArgs"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/> is <c>null</c>.</exception>
   public static TStatement CreateStatementBuilder<TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality>( this Connection<TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality> connection, TStatementCreationArgs creationArgs )
      where TVendorFunctionality : ConnectionVendorFunctionality<TStatement, TStatementCreationArgs>
      where TStatement : TStatementInformation
   {
      return connection.VendorFunctionality.CreateStatementBuilder( creationArgs );
   }

   /// <summary>
   /// This is shortcut method to directly prepare statement from its starting parameters without using builder.
   /// </summary>
   /// <typeparam name="TStatement">The type of objects used to manipulate or query remote resource.</typeparam>
   /// <typeparam name="TStatementInformation">The type of objects describing <typeparamref name="TStatement"/>s.</typeparam>
   /// <typeparam name="TStatementCreationArgs">The type of object used to create an instance of <typeparamref name="TStatement"/>.</typeparam>
   /// <typeparam name="TEnumerableItem">The type of object representing the response of manipulation or querying remote resource.</typeparam>
   /// <typeparam name="TVendorFunctionality">The type of object describing vendor-specific information.</typeparam>
   /// <param name="connection">This <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/>.</param>
   /// <param name="creationArgs">The statement builder creation parameters.</param>
   /// <returns>A new instance of <see cref="AsyncEnumerator{T}"/>, ready to be executed.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/> is <c>null</c>.</exception>
   public static AsyncEnumerator<TEnumerableItem, TStatementInformation> PrepareStatementForExecution<TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality>( this Connection<TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality> connection, TStatementCreationArgs creationArgs )
      where TVendorFunctionality : ConnectionVendorFunctionality<TStatement, TStatementCreationArgs>
      where TStatement : TStatementInformation
   {
      return connection.PrepareStatementForExecution( connection.CreateStatementBuilder( creationArgs ) );
   }

   // For some reason the implicit cast of UtilPack.EitherOr struct is not always picked up by compiler, so these two methods exist

   /// <summary>
   /// This is shortcut method to directly prepare statement from its starting parameters without using builder.
   /// </summary>
   /// <typeparam name="TStatement">The type of objects used to manipulate or query remote resource.</typeparam>
   /// <typeparam name="TStatementInformation">The type of objects describing <typeparamref name="TStatement"/>s.</typeparam>
   /// <typeparam name="TEnumerableItem">The type of object representing the response of manipulation or querying remote resource.</typeparam>
   /// <typeparam name="TVendorFunctionality">The type of object describing vendor-specific information.</typeparam>
   /// <typeparam name="T1">The first possible type of statement creation parameters.</typeparam>
   /// <typeparam name="T2">The second possible type of statement creation parameters.</typeparam>
   /// <param name="connection">This <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/>.</param>
   /// <param name="creationArgs">The statement builder creation parameters.</param>
   /// <returns>A new instance of <see cref="AsyncEnumerator{T}"/>, ready to be executed.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/> is <c>null</c>.</exception>
   public static AsyncEnumerator<TEnumerableItem, TStatementInformation> PrepareStatementForExecution<TStatement, TStatementInformation, TEnumerableItem, TVendorFunctionality, T1, T2>( this Connection<TStatement, TStatementInformation, EitherOr<T1, T2>, TEnumerableItem, TVendorFunctionality> connection, T1 creationArgs )
      where TVendorFunctionality : ConnectionVendorFunctionality<TStatement, EitherOr<T1, T2>>
      where TStatement : TStatementInformation
   {
      return connection.PrepareStatementForExecution( connection.CreateStatementBuilder( creationArgs ) );
   }

   /// <summary>
   /// This is shortcut method to directly prepare statement from its starting parameters without using builder.
   /// </summary>
   /// <typeparam name="TStatement">The type of objects used to manipulate or query remote resource.</typeparam>
   /// <typeparam name="TStatementInformation">The type of objects describing <typeparamref name="TStatement"/>s.</typeparam>
   /// <typeparam name="TEnumerableItem">The type of object representing the response of manipulation or querying remote resource.</typeparam>
   /// <typeparam name="TVendorFunctionality">The type of object describing vendor-specific information.</typeparam>
   /// <typeparam name="T1">The first possible type of statement creation parameters.</typeparam>
   /// <typeparam name="T2">The second possible type of statement creation parameters.</typeparam>
   /// <param name="connection">This <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/>.</param>
   /// <param name="creationArgs">The statement builder creation parameters.</param>
   /// <returns>A new instance of <see cref="AsyncEnumerator{T}"/>, ready to be executed.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/> is <c>null</c>.</exception>
   public static AsyncEnumerator<TEnumerableItem, TStatementInformation> PrepareStatementForExecution<TStatement, TStatementInformation, TEnumerableItem, TVendorFunctionality, T1, T2>( this Connection<TStatement, TStatementInformation, EitherOr<T1, T2>, TEnumerableItem, TVendorFunctionality> connection, T2 creationArgs )
      where TVendorFunctionality : ConnectionVendorFunctionality<TStatement, EitherOr<T1, T2>>
      where TStatement : TStatementInformation
   {
      return connection.PrepareStatementForExecution( connection.CreateStatementBuilder( creationArgs ) );
   }


   /// <summary>
   /// This is shortcut method to prepare statement and execute it while ignoring any possibly returned results of (return values of <see cref="AsyncEnumerator{T}.OneTimeRetrieve"/> when encountered during <see cref="M:E_UtilPack.EnumerateAsync{T}(UtilPack.AsyncEnumeration.AsyncEnumerator{T},System.Func{T, System.Threading.Tasks.Task})"/> method).
   /// </summary>
   /// <typeparam name="TStatement">The type of objects used to manipulate or query remote resource.</typeparam>
   /// <typeparam name="TStatementInformation">The type of objects describing <typeparamref name="TStatement"/>s.</typeparam>
   /// <typeparam name="TStatementCreationArgs">The type of object used to create an instance of <typeparamref name="TStatement"/>.</typeparam>
   /// <typeparam name="TEnumerableItem">The type of object representing the response of manipulation or querying remote resource.</typeparam>
   /// <typeparam name="TVendorFunctionality">The type of object describing vendor-specific information.</typeparam>
   /// <param name="connection">This <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/>.</param>
   /// <param name="statement">The statement builder.</param>
   /// <param name="action">Optional synchronous callback to execute after execution has started, and before it is ended.</param>
   /// <returns>A task which will have enumerated the <see cref="AsyncEnumerator{T}"/> returned by <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}.PrepareStatementForExecution(TStatementInformation)"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/> is <c>null</c>.</exception>
   public static ValueTask<Int64> ExecuteAndIgnoreResults<TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality>( this Connection<TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality> connection, TStatement statement, Action action = null )
      where TVendorFunctionality : ConnectionVendorFunctionality<TStatement, TStatementCreationArgs>
      where TStatement : TStatementInformation
   {
      var enumerator = connection.PrepareStatementForExecution( statement );
      if ( action != null )
      {
         enumerator.BeforeEnumerationEnd += ( args ) => action();
      }
      return enumerator.EnumeratePreferParallel( (Action<TEnumerableItem>) null );
   }

   /// <summary>
   /// This is shortcut method to create statement builder from creation parameters, prepare statement builder for execution, and execute it while ignoring any possibly returned results of (return values of <see cref="AsyncEnumerator{T}.OneTimeRetrieve"/> when encountered during <see cref="M:E_UtilPack.EnumerateAsync{T}(UtilPack.AsyncEnumeration.AsyncEnumerator{T},System.Func{T, System.Threading.Tasks.Task})"/> method).
   /// </summary>
   /// <typeparam name="TStatement">The type of objects used to manipulate or query remote resource.</typeparam>
   /// <typeparam name="TStatementInformation">The type of objects describing <typeparamref name="TStatement"/>s.</typeparam>
   /// <typeparam name="TStatementCreationArgs">The type of object used to create an instance of <typeparamref name="TStatement"/>.</typeparam>
   /// <typeparam name="TEnumerableItem">The type of object representing the response of manipulation or querying remote resource.</typeparam>
   /// <typeparam name="TVendorFunctionality">The type of object describing vendor-specific information.</typeparam>
   /// <param name="connection">This <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/>.</param>
   /// <param name="creationArgs">The creation parameters for statement builder.</param>
   /// <param name="action">Optional synchronous callback to execute after execution has started, and before it is ended.</param>
   /// <returns>A task which will have enumerated the <see cref="AsyncEnumerator{T}"/> returned by <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}.PrepareStatementForExecution(TStatementInformation)"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="Connection{TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality}"/> is <c>null</c>.</exception>
   public static ValueTask<Int64> ExecuteAndIgnoreResults<TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality>( this Connection<TStatement, TStatementInformation, TStatementCreationArgs, TEnumerableItem, TVendorFunctionality> connection, TStatementCreationArgs creationArgs, Action action = null )
      where TVendorFunctionality : ConnectionVendorFunctionality<TStatement, TStatementCreationArgs>
      where TStatement : TStatementInformation
   {
      return connection.ExecuteAndIgnoreResults( connection.CreateStatementBuilder( creationArgs ), action );
   }

}