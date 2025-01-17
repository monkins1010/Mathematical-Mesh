﻿#region // Copyright - MIT License
//  © 2021 by Phill Hallam-Baker
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
#endregion

namespace Goedel.Utilities;

/// <summary>
/// Static class containing extension methods for array manipulation.
/// </summary>
public static class ArrayUtilities {


    /// <summary>
    /// Returns <code>true</code> iff the intersection of <paramref name="list1"/> and <paramref name="list2"/>
    /// is non-empty.
    /// </summary>
    /// <typeparam name="T">Any object that supports testing for equality.</typeparam>
    /// <param name="list1">The first list to compare.</param>
    /// <param name="list2">The second list to compare.</param>
    /// <returns><code>true</code> iff the intersection of <paramref name="list1"/> and <paramref name="list2"/>
    /// is non-empty; otherwise, <code>false</code>.</returns>
    public static bool Intersects<T>(this IEnumerable<T> list1, IEnumerable<T> list2) where T : IEquatable<T> {

        if (list1 == null | list2 == null) {
            return false;
            }

        foreach (var item1 in list1) {

            foreach (var item2 in list2) {
                if (item1.Equals(item2)) {
                    return true;
                    }
                }
            }


        return false;
        }



    /// <summary>Test to see if two arrays are equal.
    /// </summary>
    /// <param name="test1">First test value</param>
    /// <param name="test2">Second test value</param>
    /// <returns>true if and only if the two arrays are of the same size and each
    /// element is equal.</returns>
    public static bool IsEqualTo(this byte[] test1, byte[] test2) {
        if ((test1 == null) & (test2 == null)) {
            return true;
            }
        if (test2 == null) {
            return false;
            }
        if (test1 == null) {
            return false;
            }
        if (test1.Length != test2.Length) {
            return false;
            }
        for (int i = 0; i < test1.Length; i++) {
            if (test1[i] != test2[i]) {
                return false;
                }
            }

        return true;
        }

    /// <summary>
    /// Copy data into the destination array at the specified index. If there is insufficient
    /// space, the remaining data is silently discarded. The main use for this is for inside 
    /// key derivation functions where the amount of key generated by the function may be greater
    /// than the amount needed.
    /// </summary>
    /// <param name="destination">The destination array</param>
    /// <param name="offset">Offset in the destination array</param>
    /// <param name="data">Data to be written</param>
    /// <returns>Offset of the next byte to be written.</returns>
    public static int AppendChecked(this byte[] destination, int offset, byte[] data) {
        if (offset > destination.Length) {
            return destination.Length;
            }

        var Length = offset + data.Length <= destination.Length ?
                  data.Length : destination.Length - offset; // remaining space

        Array.Copy(data, 0, destination, offset, Length);
        return offset + Length;
        }

    /// <summary>
    /// Create a duplicate copy of a byte array. This allows the original data to be modified
    /// or disposed of.
    /// </summary>
    /// <param name="source">The source array</param>
    /// <returns>The copied array</returns>
    public static byte[] Duplicate(this byte[] source) {
        var Result = new byte[source.Length];
        Array.Copy(source, Result, source.Length);
        return Result;
        }

    /// <summary>
    /// Conditional truncation of an output value. If the value Length is zero,
    /// returns the source array. Otherwise a new array is created and the first
    /// Length bits of the source array copied into it and the new array returned.
    /// </summary>
    /// <param name="source">The source array</param>
    /// <param name="length">The truncation length, 0 for no truncation.</param>
    /// <returns>Truncated value</returns>
    public static byte[] OrTruncated(this byte[] source, int length) {
        if (length <= 0) {
            return Duplicate(source);
            }
        length /= 8; // Convert to bytes
        var Result = new byte[length];
        Array.Copy(source, Result, length);

        return Result;
        }


    /// <summary>
    /// Wrapper for the Dictionary Add method to force replacement of the previous value if it exists.
    /// </summary>
    /// <typeparam name="TKey">The Key type</typeparam>
    /// <typeparam name="TValue">The Value type, null values are pruned.</typeparam>
    /// <param name="dictionary">The dictionary to add to</param>
    /// <param name="key">The key to add</param>
    /// <param name="value">The value to add</param>
    /// <returns>True if a new entry was added, otherwise false.</returns>
    public static void Replace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) {
        if (dictionary.ContainsKey(key)) {
            dictionary.Remove(key);
            }
        dictionary.Add(key, value);
        }


    /// <summary>
    /// Wrapper for the Dictionary Add method to signal success or failure by means of a
    /// boolean return value rather than throwing an error.
    /// </summary>
    /// <typeparam name="TKey">The Key type</typeparam>
    /// <typeparam name="TValue">The Value type, null values are pruned.</typeparam>
    /// <param name="dictionary">The dictionary to add to</param>
    /// <param name="key">The key to add</param>
    /// <param name="value">The value to add</param>
    /// <returns>True if a new entry was added, otherwise false.</returns>
    public static bool AddSafe<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) {
        if (value == null) {
            return false;
            }

        try {
            dictionary.Add(key, value);
            return true;
            }
        catch (System.ArgumentException) {
            return false;
            }

        }

    /// <summary>
    /// Wrapper for the Dictionary TryGetValue method returning the found value as the function result instead of 
    /// the success or failure.
    /// </summary>
    /// <typeparam name="TKey">The Key type</typeparam>
    /// <typeparam name="TValue">The Value type.</typeparam>
    /// <param name="dictionary">The dictionary to search</param>
    /// <param name="key">The key to search on</param>
    /// <param name="defaultValue">The default value to return if nothing is found.</param>
    /// <returns>The value found.</returns>
    public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) {
        var found = dictionary.TryGetValue(key, out var result);
        return found ? result : defaultValue;
        }


    /// <summary>
    /// Wrapper for the Dictionary TryGetValue method returning the found value as the function result instead of 
    /// the success or failure.
    /// </summary>
    /// <typeparam name="TKey">The Key type</typeparam>
    /// <typeparam name="TValue">The Value type.</typeparam>
    /// <param name="dictionary">The dictionary to search</param>
    /// <param name="key">The key to search on</param>
    /// <returns>The value found.</returns>
    public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
        dictionary.TryGetValue(key, out var result);
        return result;
        }

    /// <summary>
    /// Wrapper for the Dictionary Add method to signal success or failure by means of a
    /// boolean return value rather than throwing an error.
    /// </summary>
    /// <typeparam name="TKey">The Key type</typeparam>
    /// <typeparam name="TValue">The Value type, null values are pruned.</typeparam>
    /// <param name="dictionary">The dictionary to add to</param>
    /// <param name="key">The key to add</param>
    /// <param name="value">The value to add</param>
    /// <returns>True if a new entry was added, otherwise false.</returns>
    public static bool ReplaceSafe<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) {
        if (value == null) {
            return false;
            }

        try {
            dictionary.Remove(key);
            dictionary.Add(key, value);
            return true;
            }
        catch {
            return false;
            }

        }


    /// <summary>
    /// Concatenate the two arrays
    /// </summary>
    /// <param name="first">First array</param>
    /// <param name="second">Second array</param>
    /// <returns>result</returns>
    public static byte[] Concatenate(this byte[] first, byte[] second) {
        var Buffer = new byte[first.Length + second.Length];
        Array.Copy(first, Buffer, first.Length);
        Array.Copy(second, 0, Buffer, first.Length, second.Length);
        return Buffer;
        }


    /// <summary>
    /// Return element number <paramref name="index"/> from <paramref name="list"/>
    /// if it exists, otherwise the default for the list element type.
    /// </summary>
    /// <typeparam name="T">The list element type.</typeparam>
    /// <param name="list">The list to extract from (may be null).</param>
    /// <param name="index">The element to extract (if present).</param>
    /// <returns>The extracted element or null if either <paramref name="list"/> is null
    /// or <paramref name="index"/> is greater than the number of elements.</returns>
    public static T SafeIndex<T>(this List<T> list, int index = 0) =>
        list == null || list.Count <= index ? default : list[index];

    /// <summary>
    /// If the list <paramref name="second"/> is not null, append the entries to the list
    /// <paramref name="first"/>. Otherwise, do nothing.
    /// </summary>
    /// <typeparam name="T">The type of the list entries.</typeparam>
    /// <param name="first">The list to add to.</param>
    /// <param name="second">The list of items to be added if not null.</param>
    public static void AddRangeSafe<T>(this List<T> first, List<T> second) {
        if (second != null) {
            first.AddRange(second);
            }

        }


    }
