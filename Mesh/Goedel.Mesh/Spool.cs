﻿using Goedel.Cryptography;
using Goedel.Cryptography.Dare;
using Goedel.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Goedel.Mesh {

    ///<summary>Message entry in spool catalog</summary>
    public class SpoolEntry {

        ///<summary>The spool the message is enrolled in.</summary>
        public Spool Spool { get;}

        ///<summary>The unique envelope identifier.</summary>
        public string EnvelopeID { get; private set; }

        ///<summary>The envelope from the spool.</summary>
        public DareEnvelope DareEnvelope { get; private set; }

        ///<summary>The next entry in the spool.</summary>
        public SpoolEntry Next { get; private set; }

        ///<summary>The previous entry in the spool.</summary>
        public SpoolEntry Previous { get; private set; }


        public bool Closed => (MessageStatus & MessageStatus.Closed) == MessageStatus.Closed;
        public bool Open => (MessageStatus & MessageStatus.Open) == MessageStatus.Open;


        public List<Reference> References;

        ///<summary>Returns the message</summary>
        public Message Message => message ?? Decode().CacheValue(out message);
        Message message;



        public long Index => DareEnvelope.Index;

        public MessageStatus MessageStatus;


        public Exception Exception { get; private set; }

        public SpoolEntry(Spool spool, DareEnvelope envelope, SpoolEntry next) {
            Spool = spool;
            MessageStatus = MessageStatus.Open;
            EnvelopeID = envelope.EnvelopeID;
            AddEnvelope(envelope, next);
            }

        public SpoolEntry(Spool spool, Reference reference) {
            EnvelopeID = reference.MessageID;
            }


        /// <summary>
        /// Add an envelope to an existing entry created because a status value was reported.
        /// </summary>
        /// <param name="envelope">The envelope to add.</param>
        /// <param name="next">The next entry in the spool.</param>
        public void AddEnvelope(DareEnvelope envelope, SpoolEntry next) {
            DareEnvelope = envelope;
            Link(next);
            }

        public void Link(SpoolEntry next) {
            if (next == null) {
                return;
                }
            Next = next;
            next.Previous = this;
            }




        public void AddReference(Reference reference, bool force) {
            if ((References == null) | force) {
                References ??= new List<Reference>();
                References.Insert(0, reference);
                MessageStatus = reference.MessageStatus;
                }
            else {
                References.Add(reference);
                }
            }


        /// <summary>
        /// Decode the envelope as a DARE Message using the current
        /// KeyCollection and return the result.
        /// <returns>The decoded message</returns>
        Message Decode() {
            if (DareEnvelope.JSONObject != null) {
                return DareEnvelope.JSONObject as Message;
                }

            DareEnvelope.JSONObject = Message.Decode(DareEnvelope, Spool.KeyCollection);
            return DareEnvelope.JSONObject as Message;
            }


        }

    /// <summary>
    /// Base class for stores of type Spool.
    /// </summary>
    public class Spool : Store {

        ///<summary>The last spool entry.</summary>
        SpoolEntry SpoolEntryLast { get; set; } = null;

        ///<summary>Dictionary of entries by identifier.</summary>
        Dictionary<string, SpoolEntry> SpoolEntryById { get; }  = new Dictionary<string, SpoolEntry>();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="directory">The directory in which the spool is stored.</param>
        /// <param name="storeName">The store name.</param>
        /// <param name="cryptoParameters">The cryptographic parameters.</param>
        /// <param name="keyCollection">The key collection to fetch keys from.</param>
        public Spool(string directory, string storeName,
            CryptoParameters cryptoParameters = null,
                    KeyCollection keyCollection = null) :
                base(directory, storeName, cryptoParameters, keyCollection) {

            }

        /// <summary>
        /// Return the status of the spool.
        /// </summary>
        /// <param name="directory">The directory in which the spool is stored.</param>
        /// <param name="storeName">The store name.</param>
        /// <returns></returns>
        public static ContainerStatus Status(string directory, string storeName) {
            using var store = new Spool(directory, storeName);
            return new ContainerStatus() {
                Index = (int)store.Container.FrameCount,
                Container = storeName
                };
            }

        /// <summary>
        /// Add an envelope to the spool. All information provided in the ContainerInfo
        /// field is discarded. The trailer, if present must be rewritten for the 
        /// purposes of the container.
        /// </summary>
        /// <param name="dareMessage"></param>
        public SpoolEntry Add(DareEnvelope dareMessage) {
            // Have to bite the bullet now and write the correct scheme.

            throw new NYI();

            // NYI: If the message is signed or encrypted, these enhancements should be carried over
            Container.Append(dareMessage.Body, null, dareMessage.Header.ContentMeta);

            return Intern(dareMessage, null);
            }

        /// <summary>
        /// Check that the time value <paramref name="dateTime"/> is within the boundaries
        /// defined by <paramref name="maxTicks"/>, <paramref name="notBefore"/> and
        /// <paramref name="notOnOrAfter"/>.
        /// </summary>
        /// <param name="dateTime">The time to test.</param>
        /// <param name="maxTicks">If greater or equal to zero, return <code>false</code> if 
        /// <paramref name="dateTime"/> is more than
        /// this number of ticks earlier than the current time.</param>
        /// <param name="notBefore">If not null, return <code>false</code> if <paramref name="dateTime"/>
        /// is earlier than this time.</param>
        /// <param name="notOnOrAfter">If not null, return <code>false</code> if <paramref name="dateTime"/>
        /// is later than or the same as this time.</param>
        /// <returns><code>true</code> unless one of the conditions defined by <paramref name="maxTicks"/>, 
        /// <paramref name="notBefore"/> or <paramref name="notOnOrAfter"/> </returns> is not
        /// met in which case return <code>false</code>.
        public bool CheckTime(
                    DateTime? dateTime,
                    long maxTicks = -1,
                    DateTime? notBefore = null,
                    DateTime? notOnOrAfter = null) {
            dateTime.AssertNotNull();

            var dateTime1 = (DateTime)dateTime;

            if ((maxTicks >= 0) && ((DateTime.Now.Ticks - dateTime1.Ticks) > maxTicks)) {
                return false;
                }

            if ((notBefore != null) && (dateTime1 < notBefore)) {
                return false;
                }
            if ((notOnOrAfter != null) && (dateTime1 >= notOnOrAfter)) {
                return false;
                }
            return true;
            }

        ///// <summary>
        ///// Check that the envelope <paramref name="dareEnvelope"/> is within the boundaries
        ///// defined by <paramref name="maxTicks"/>, <paramref name="notBefore"/> and
        ///// <paramref name="notOnOrAfter"/>.
        ///// </summary>
        ///// <param name="dareEnvelope">The envelope to test.</param>
        ///// <param name="select">Message status selection mask.</param>
        ///// <param name="maxTicks">If greater or equal to zero, return <code>false</code> if 
        ///// the time the envelope was last modified is more than
        ///// this number of ticks earlier than the current time.</param>
        ///// <param name="notBefore">If not null, return <code>false</code> if the time the envelope was last modified
        ///// is earlier than this time.</param>
        ///// <param name="notOnOrAfter">If not null, return <code>false</code> if the time the envelope was last modified
        ///// is later than or the same as this time.</param>
        ///// <returns><code>true</code> unless one of the conditions defined by <paramref name="maxTicks"/>, 
        ///// <paramref name="notBefore"/> or <paramref name="notOnOrAfter"/> </returns> is not
        ///// met in which case return <code>false</code>.
        //public bool CheckEnvelope(
        //            DareEnvelope dareEnvelope,
        //            MessageStatus select = MessageStatus.All,
        //            long maxTicks = -1,
        //            DateTime? notBefore = null,
        //            DateTime? notOnOrAfter = null) {

        //    if (dareEnvelope == null) {
        //        return false;
        //        }

        //    var time = dareEnvelope.Header.ContentMeta.Modified;
        //    var id = dareEnvelope.Header.ContentMeta.UniqueID;

        //    if (!statusById.TryGetValue(id, out var messageStatus)) {
        //        messageStatus = MessageStatus.Initial;
        //        }

        //    var expire = dareEnvelope.Header.ContentMeta.Expire;
        //    if (expire != null && (expire < DateTime.Now)) {
        //        messageStatus |= MessageStatus.Expired;
        //        }
        //    else {
        //        messageStatus |= MessageStatus.Unexpired;
        //        }

        //    // check the selection criteria
        //    if ((messageStatus & select) != select) {
        //        return false;
        //        }

        //    return CheckTime(time, maxTicks, notBefore, notOnOrAfter);

        //    }

        /// <summary>
        /// Retrieve a message by message ID. 
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="select"></param>
        /// <param name="maxTicks"></param>
        /// <param name="notBefore"></param>
        /// <param name="notOnOrAfter"></param>
        /// <param name="maxSearch"></param>
        /// <returns></returns>
        public SpoolEntry GetByMessageId(
                    string messageID,
                    MessageStatus select = MessageStatus.All,
                    DateTime? notBefore = null,
                    DateTime? notOnOrAfter = null,
                    long maxSearch = -1) => GetByEnvelopeId(Message.GetEnvelopeID(messageID),
                        select, notBefore, notOnOrAfter);


        /// <summary>
        /// Retrieve a message by message ID. 
        /// </summary>
        /// <param name="envelopeId"></param>
        /// <param name="select"></param>
        /// <param name="notBefore"></param>
        /// <param name="notOnOrAfter"></param>
        /// <param name="maxSearch"></param>
        /// <returns></returns>
        public SpoolEntry GetByEnvelopeId(
                    string envelopeId,
                    MessageStatus select = MessageStatus.All,
                    DateTime? notBefore = null,
                    DateTime? notOnOrAfter = null,
                    long maxSearch = -1) {


            if (SpoolEntryById.TryGetValue(envelopeId, out var spoolEntry)) {
                return spoolEntry;
                }

            foreach (var spoolEntry2 in GetMessages(select, notBefore, notOnOrAfter, 
                        SpoolEntryLast, maxSearch)) {
                if (spoolEntry2.EnvelopeID == envelopeId) {
                    return spoolEntry2;
                    }
                }
            return null;
            }



        /// <summary>
        /// Returns an enumerator over the messages in the spool that match the
        /// constraints specified by <paramref name="include"/>, <paramref name="exclude"/>,
        /// <paramref name="maxTicks"/>, <paramref name="notBefore"/>, <paramref name="notOnOrAfter"/>
        /// and <paramref name="maxResults"/>.
        /// <para>A message meets the criteria specified by the message inclusion and exclusion masks
        /// if and only if (<code>MessageStatus</code> &amp; <paramref name="include"/> != MessageStatus.None)
        /// &amp; (<code>MessageStatus</code> &amp; <paramref name="exclude"/> == MessageStatus.None) </para>. That
        /// is, there must be at least one match to the inclusion mask and no matches to the exclusion
        /// mask.
        /// </summary>
        /// <param name="select">Message status selection mask.</param>
        /// <param name="maxTicks">Maximum message age in ticks (-1 = check all).</param>
        /// <param name="notBefore">Exclude messages sent before this time.</param>
        /// <param name="notOnOrAfter">Exclude messages sent after this time.</param>
        /// <param name="maxSearch">Starting point for the search, i.e. the highest frame number</param> 
        /// <param name="minSearch">Ending point for the search, i.e. the lowest frame number</param> 
        /// <param name="maxResults">Maximum number of records to check (-1 = check all).</param>
        /// <returns>The enumerator.</returns>
        public SpoolEnumeratorRaw GetMessages(
                    MessageStatus select = MessageStatus.All,
                    DateTime? notBefore = null,
                    DateTime? notOnOrAfter = null,
                    SpoolEntry last = null,
                    long maxResults = -1) => new SpoolEnumeratorRaw(this,
                        select, notBefore, notOnOrAfter, last, maxResults);


        /// <summary>
        /// Intern <paramref name="envelope"/> with the following envelope 
        /// <paramref name="next"/>.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="next">The next frame number</param>
        /// <returns>The interned envelope instance.</returns>
        SpoolEntry Intern(DareEnvelope envelope, SpoolEntry next) {
            if (envelope == null) {
                return null;
                }

            if (SpoolEntryById.TryGetValue(envelope.EnvelopeID, out var spoolEntry)) {
                spoolEntry.AddEnvelope(envelope, next);

                }
            else {
                spoolEntry = new SpoolEntry(this, envelope, next);
                SpoolEntryById.Add(spoolEntry.EnvelopeID, spoolEntry);

                // If this is the last entry in the spool, set the high water mark.
                if (next == null) {
                    if (SpoolEntryLast != null) {
                        SpoolEntryLast.Link(spoolEntry);
                        }
                    SpoolEntryLast = spoolEntry;
                    }
                }

            if (KeyCollection == null) {
                // do nothing, we can't read the contents of the message
                }
            else if (envelope.Header.ContentMeta.MessageType == MessageComplete.__Tag) {
                var message = spoolEntry.Message;
                message.AssertNotNull();  // Hack - need to collect up the errors 
                foreach (var reference in message.References) {
                    // Do we already have an entry?
                    var envelopeID = reference.EnvelopeID;
                    if (SpoolEntryById.TryGetValue(envelopeID, out var referenceEntry)) {
                        referenceEntry.AddReference(reference, next==null);
                        }
                    else {
                        referenceEntry = new SpoolEntry(this, reference);
                        SpoolEntryById.Add(envelopeID, referenceEntry);
                        }
                    }
                }

            return spoolEntry;

            }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public SpoolEntry GetPrevious(SpoolEntry current) {
            if (current == null) {
                return GetLast();
                }

            if (current.Previous != null) {
                return current.Previous;
                }

            if (!Container.MoveToIndex(current.Index)) {// not found?
                return null;
                }

            var envelope = Container.ReadDirectReverse();

            return Intern(envelope, current);
            }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SpoolEntry GetLast() {

            if (SpoolEntryLast != null) {
                return SpoolEntryLast;
                }

            if (!Container.MoveToLast()) {  // not found?

                return null;
                }

            var envelope = Container.ReadDirectReverse();

            return Intern(envelope, null);
            }


        }

    /// <summary>
    /// Enumerator that returns the raw, unencrypted container data.
    /// </summary>
    public class SpoolEnumeratorRaw : IEnumerator<SpoolEntry> {

        // Parameters passed in from search criteria.
        private readonly Spool spool;
        private readonly MessageStatus select;
        private readonly long maxTicks;

        private readonly long maxResults;
        private readonly SpoolEntry last;

        // Mask parameters.
        private bool checkMaxResults;


        // Local variables
        private long results;

        ///<summary>The current enumerated value.</summary>
        public SpoolEntry Current { get; private set; } = null;

        /// <summary>
        /// When called on an instance of this class, returns the instance. Thus allowing
        /// selectors to be used in sub classes.
        /// </summary>
        /// <returns>This instance</returns>
        public SpoolEnumeratorRaw GetEnumerator() => this;

        object IEnumerator.Current => Current;

        /// <summary>
        /// Constructor for an enumerator on the store <paramref name="spool"/> with search constraints.
        /// 
        /// 
        /// </summary>
        /// <remarks>This enumerator is NOT currently thread safe though it should be.</remarks>
        /// <param name="spool"></param>
        /// <param name="select"></param>
        /// <param name="notBefore"></param>
        /// <param name="notOnOrAfter"></param>
        /// <param name="last"></param>

        /// <param name="maxResults"></param>
        public SpoolEnumeratorRaw(
                    Spool spool,
                    MessageStatus select = MessageStatus.All,
                    DateTime? notBefore = null,
                    DateTime? notOnOrAfter = null,
                    SpoolEntry last = null,
                    long maxResults = -1) {
            this.spool = spool;
            this.select = select;
            this.last = last;
            this.maxResults = maxResults;

            checkMaxResults = maxResults >= 0;

            Reset();
            MoveNext();

            }

        public void Dispose() {
            }



        public bool MoveNext() {
            while (true) {

                // Check the conditions for continuing to search
                if (checkMaxResults && (results >= maxResults)) {
                    Current = null;
                    return false;
                    }

                // move to the next item
                Current = spool.GetPrevious(Current);

                // do we meet the selection criteria?
                if (Current != null) {
                    results++;
                    return true;
                    }

                return false;
                }

            }

        public void Reset() {
            results = 0;
            


            Current = last;
            }



        }


    /// <summary>
    /// Class for the local spool
    /// </summary>
    public class SpoolLocal : Spool {

        ///<summary>Canonical name for local spool</summary>
        public const string Label = "mmm_Local";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="directory">The directory in which the spool is stored.</param>
        /// <param name="storeName">The store name.</param>
        /// <param name="cryptoParameters">The cryptographic parameters.</param>
        /// <param name="keyCollection">The key collection to fetch keys from.</param>
        public SpoolLocal(string directory, string storeName,
            CryptoParameters cryptoParameters = null,
                    KeyCollection keyCollection = null) :
                base(directory, storeName, cryptoParameters, keyCollection) {

            }

        /// <summary>
        /// Check the spool for a matching unexpired PIN identifier and return the plaintext of the
        /// first unexpired message.
        /// </summary>
        /// <param name="pinID">The identifier of the corresponding PIN value.</param>
        /// <param name="maxTicks">Maximum message age in ticks (-1 = check all).</param>
        /// <param name="maxSearch">Maximum number of records to check (-1 = check all).</param>
        /// <returns>The plaintext message.</returns>
        public Message CheckPIN(string pinID, 
                    long maxTicks = -1,
                    long maxSearch = -1) {

            throw new NYI();

            //// Get all the unexpired messages that match the PIN identifier
            //var envelope = GetByMessageID(pinID, MessageStatus.Unexpired,
            //            maxTicks:maxTicks, maxSearch:maxSearch);

            //// Return the decrypted message
            //return Message.Decode(envelope, KeyCollection);
            }

        }

    /// <summary>
    /// Class for the inbound spool
    /// </summary>
    public class SpoolInbound: Spool {

        ///<summary>Canonical name for inbound spool</summary>
        public const string Label = "mmm_Inbound";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="directory">The directory in which the spool is stored.</param>
        /// <param name="storeName">The store name.</param>
        /// <param name="cryptoParameters">The cryptographic parameters.</param>
        /// <param name="keyCollection">The key collection to fetch keys from.</param>
        public SpoolInbound(string directory, string storeName,
            CryptoParameters cryptoParameters = null,
                    KeyCollection keyCollection = null) :
                base(directory, storeName, cryptoParameters, keyCollection) {

            }
        }

    /// <summary>
    /// Class for the outbound spool
    /// </summary>
    public class SpoolOutbound : Spool {

        ///<summary>Canonical name for outbound spool</summary>
        public const string Label = "mmm_Outbound";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="directory">The directory in which the spool is stored.</param>
        /// <param name="storeName">The store name.</param>
        /// <param name="cryptoParameters">The cryptographic parameters.</param>
        /// <param name="keyCollection">The key collection to fetch keys from.</param>
        public SpoolOutbound(string directory, string storeName,
            CryptoParameters cryptoParameters = null,
                    KeyCollection keyCollection = null) :
                base(directory, storeName, cryptoParameters, keyCollection) {

            }
        }

    /// <summary>
    /// Class for the outbound spool
    /// </summary>
    public class SpoolArchive : Spool {

        ///<summary>Canonical name for outbound spool</summary>
        public const string Label = "mmm_Archive";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="directory">The directory in which the spool is stored.</param>
        /// <param name="storeName">The store name.</param>
        /// <param name="cryptoParameters">The cryptographic parameters.</param>
        /// <param name="keyCollection">The key collection to fetch keys from.</param>
        public SpoolArchive(string directory, string storeName,
            CryptoParameters cryptoParameters = null,
                    KeyCollection keyCollection = null) :
                base(directory, storeName, cryptoParameters, keyCollection) {

            }
        }


    }
