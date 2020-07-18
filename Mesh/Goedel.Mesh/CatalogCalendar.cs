﻿using Goedel.Cryptography;
using Goedel.Cryptography.Dare;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Goedel.Mesh {


    #region // The data classes CatalogCalendar, CatalogedTask

    /// <summary>
    /// Calendar catalog. Describes the tasks in a Mesh account.
    /// </summary>
    public class CatalogCalendar : Catalog<CatalogedTask> {

        ///<summary>The canonical label for the catalog</summary>
        public const string Label = "mmm_Calendar";

        ///<summary>The catalog label</summary>
        public override string ContainerDefault => Label;

        /// <summary>
        /// Constructor for a catalog named <paramref name="storeName"/> in directory
        /// <paramref name="directory"/> using the cryptographic parameters <paramref name="cryptoParameters"/>
        /// and key collection <paramref name="keyCollection"/>.
        /// </summary>
        /// <param name="create">Create a new persistence store on disk if it does not already exist.</param>
        /// <param name="decrypt">Attempt to decrypt the contents of the catalog if encrypted.</param>
        /// <param name="directory">The directory in which the catalog persistence container is stored.</param>
        /// <param name="storeName">The catalog persistence container file name.</param>
        /// <param name="cryptoParameters">The default cryptographic enhancements to be applied to container entries.</param>
        /// <param name="keyCollection">The key collection to be used to resolve keys when reading entries.</param>
        public CatalogCalendar(
                    string directory,
                    string storeName = null,
                    CryptoParameters cryptoParameters = null,
                    IKeyCollection keyCollection = null,
                    bool decrypt = true,
                    bool create = true) :
            base(directory, storeName ?? Label,
                        cryptoParameters, keyCollection, decrypt: decrypt, create: create) {
            }

        }

    // NYI should all be DareMessages to allow them to be signed.


    public partial class CatalogedTask {

        /// <summary>
        /// The primary key used to catalog the entry. 
        /// </summary>
        public override string _PrimaryKey => Key;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CatalogedTask() => Key = UDF.Nonce();

        /// <summary>
        /// Constructor creating a task from the enveloped task <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task to create.</param>
        public CatalogedTask(DareEnvelope task) : this() => EnvelopedTask = task;

        /// <summary>
        /// Constructor creating a task from the task <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task to create.</param>
        public CatalogedTask(Task task) : this() => EnvelopedTask = DareEnvelope.Encode(task.GetBytes(tag: true),
                    contentType: "application/mmm");

        }
    #endregion

    }
