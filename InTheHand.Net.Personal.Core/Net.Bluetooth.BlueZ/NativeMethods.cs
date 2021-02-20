// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.NativeMethods
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using uint8_t = System.Byte;
using uint16_t = System.UInt16;
using uint32_t = System.UInt32;
using size_t = System.IntPtr;
//
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    static class NativeMethods
    {
        const string LibraryName = "bluetooth";

        #region hci_lib.h
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern int hci_open_dev(int dev_id);
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern int hci_close_dev(int dd);
        //int hci_send_cmd(int dd, uint16_t ogf, uint16_t ocf, uint8_t plen, void *param);
        //int hci_send_req(int dd, struct hci_request *req, int timeout);

        //int hci_create_connection(int dd, const bdaddr_t *bdaddr, uint16_t ptype, uint16_t clkoffset, uint8_t rswitch, uint16_t *handle, int to);
        //int hci_disconnect(int dd, uint16_t handle, uint8_t reason, int to);

        [DllImport(LibraryName, SetLastError = true)]
        internal static extern int hci_inquiry(int dev_id, int len, int num_rsp, IntPtr lap,
            ref IntPtr /*"inquiry_info **"*/ppii, StackConsts.IREQ_int flags);
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern int hci_inquiry(int dev_id, int len, int num_rsp, byte[] lap,
            ref IntPtr /*"inquiry_info **"*/ppii, StackConsts.IREQ_int flags);
        //int hci_devinfo(int dev_id, struct hci_dev_info *di);
        //int hci_devba(int dev_id, bdaddr_t *bdaddr);
        //int hci_devid(const char *str);

        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern BluezError hci_read_local_name(int dd, int len, byte[] name, int to);
        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern BluezError hci_write_local_name(int dd, byte[] name, int to);
        [Obsolete("to D-Bus")]
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern BluezError hci_read_remote_name(int dd, byte[] bdaddr, int len, byte[] nameBuf, int to);
        //int hci_read_remote_name_with_clock_offset(int dd, const bdaddr_t *bdaddr, uint8_t pscan_rep_mode, uint16_t clkoffset, int len, char *name, int to);
        //int hci_read_remote_name_cancel(int dd, const bdaddr_t *bdaddr, int to);
        //int hci_read_remote_version(int dd, uint16_t handle, struct hci_version *ver, int to);
        //int hci_read_remote_features(int dd, uint16_t handle, uint8_t *features, int to);
        //int hci_read_remote_ext_features(int dd, uint16_t handle, uint8_t page, uint8_t *max_page, uint8_t *features, int to);
        //int hci_read_clock_offset(int dd, uint16_t handle, uint16_t *clkoffset, int to);
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern BluezError hci_read_local_version(int dd, ref Structs.hci_version ver, int to);
        //int hci_read_local_commands(int dd, uint8_t *commands, int to);
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern BluezError hci_read_local_features(int dd, byte[] features, int to);
        //int hci_read_local_ext_features(int dd, uint8_t page, uint8_t *max_page, uint8_t *features, int to);
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern BluezError hci_read_bd_addr(int dd, byte[] bdaddr, int to);
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern BluezError hci_read_class_of_dev(int dd, byte[] cls, int to);
        //int hci_write_class_of_dev(int dd, uint32_t cls, int to);
        //int hci_read_voice_setting(int dd, uint16_t *vs, int to);
        //int hci_write_voice_setting(int dd, uint16_t vs, int to);
        //int hci_read_current_iac_lap(int dd, uint8_t *num_iac, uint8_t *lap, int to);
        //int hci_write_current_iac_lap(int dd, uint8_t num_iac, uint8_t *lap, int to);
        //int hci_read_stored_link_key(int dd, bdaddr_t *bdaddr, uint8_t all, int to);
        //int hci_write_stored_link_key(int dd, bdaddr_t *bdaddr, uint8_t *key, int to);
        //int hci_delete_stored_link_key(int dd, bdaddr_t *bdaddr, uint8_t all, int to);
        //int hci_authenticate_link(int dd, uint16_t handle, int to);
        //int hci_encrypt_link(int dd, uint16_t handle, uint8_t encrypt, int to);
        //int hci_change_link_key(int dd, uint16_t handle, int to);
        //int hci_switch_role(int dd, bdaddr_t *bdaddr, uint8_t role, int to);
        //int hci_park_mode(int dd, uint16_t handle, uint16_t max_interval, uint16_t min_interval, int to);
        //int hci_exit_park_mode(int dd, uint16_t handle, int to);
        //int hci_read_inquiry_scan_type(int dd, uint8_t *type, int to);
        //int hci_write_inquiry_scan_type(int dd, uint8_t type, int to);
        //int hci_read_inquiry_mode(int dd, uint8_t *mode, int to);
        //int hci_write_inquiry_mode(int dd, uint8_t mode, int to);
        //int hci_read_afh_mode(int dd, uint8_t *mode, int to);
        //int hci_write_afh_mode(int dd, uint8_t mode, int to);
        //int hci_read_ext_inquiry_response(int dd, uint8_t *fec, uint8_t *data, int to);
        //int hci_write_ext_inquiry_response(int dd, uint8_t fec, uint8_t *data, int to);
        //int hci_read_simple_pairing_mode(int dd, uint8_t *mode, int to);
        //int hci_write_simple_pairing_mode(int dd, uint8_t mode, int to);
        //int hci_read_local_oob_data(int dd, uint8_t *hash, uint8_t *randomizer, int to);
        //int hci_read_transmit_power_level(int dd, uint16_t handle, uint8_t type, int8_t *level, int to);
        //int hci_read_link_supervision_timeout(int dd, uint16_t handle, uint16_t *timeout, int to);
        //int hci_write_link_supervision_timeout(int dd, uint16_t handle, uint16_t timeout, int to);
        //int hci_set_afh_classification(int dd, uint8_t *map, int to);
        //int hci_read_link_quality(int dd, uint16_t handle, uint8_t *link_quality, int to);
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern BluezError hci_read_rssi(int dd, UInt16 handle, ref byte rssi, int to);
        //int hci_read_afh_map(int dd, uint16_t handle, uint8_t *mode, uint8_t *map, int to);
        //int hci_read_clock(int dd, uint16_t handle, uint8_t which, uint32_t *clock, uint16_t *accuracy, int to);

        //int hci_local_name(int dd, int len, char *name, int to);
        //int hci_remote_name(int dd, const bdaddr_t *bdaddr, int len, char *name, int to);

        //int hci_for_each_dev(int flag, int(*func)(int dd, int dev_id, long arg), long arg);
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern int hci_get_route(IntPtr pbdaddr);
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern int hci_get_route(byte[] bdaddr);

        //char *hci_dtypetostr(int type);
        //char *hci_dflagstostr(uint32_t flags);
        //char *hci_ptypetostr(unsigned int ptype);
        //int hci_strtoptype(char *str, unsigned int *val);
        //char *hci_scoptypetostr(unsigned int ptype);
        //int hci_strtoscoptype(char *str, unsigned int *val);
        //char *hci_lptostr(unsigned int ptype);
        //int hci_strtolp(char *str, unsigned int *val);
        //char *hci_lmtostr(unsigned int ptype);
        //int hci_strtolm(char *str, unsigned int *val);

        //char *hci_cmdtostr(unsigned int cmd);
        //char *hci_commandstostr(uint8_t *commands, char *pref, int width);

        //char *hci_vertostr(unsigned int ver);
        //int hci_strtover(char *str, unsigned int *ver);
        //char *lmp_vertostr(unsigned int ver);
        //int lmp_strtover(char *str, unsigned int *ver);

        //char *lmp_featurestostr(uint8_t *features, char *pref, int width);
        #endregion

        #region sdp_lib.h
        /*
         * SDP lists
         */
        //typedef void(*sdp_list_func_t)(void *, void *);
        internal delegate void sdp_free_func_t(IntPtr p);
        internal delegate int sdp_comp_func_t(IntPtr p1, IntPtr p2);

        [DllImport(LibraryName, SetLastError = true)]
        internal static extern IntPtr /*"sdp_list_t *"*/sdp_list_append(IntPtr /*"sdp_list_t *"*/list, IntPtr d);
        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern IntPtr /*"sdp_list_t *"*/sdp_list_remove(IntPtr /*"sdp_list_t *"*/list, IntPtr d);
        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern IntPtr /*"sdp_list_t *"*/sdp_list_insert_sorted(IntPtr /*"sdp_list_t *"*/list, IntPtr data, sdp_comp_func_t f);
        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern void sdp_list_free(IntPtr /*"sdp_list_t *"*/list, sdp_free_func_t f);

        /*
         * 	When the pdu_id(type) is a sdp error response, check the status value
         * 	to figure out the error reason. For status values 0x0001-0x0006 check
         * 	Bluetooth SPEC. If the status is 0xffff, call sdp_get_error function
         * 	to get the real reason:
         * 	    - wrong transaction ID(EPROTO)
         * 	    - wrong PDU id or(EPROTO)
         * 	    - I/O error
         */
        internal delegate void sdp_callback_t(uint8_t type, uint16_t status, ref uint8_t rsp, size_t size, IntPtr udata);

        /*
         * create an L2CAP connection to a Bluetooth device
         * 
         * INPUT:
         *  
         *  bdaddr_t *src:
         *	Address of the local device to use to make the connection
         *	(or BDADDR_ANY)
         *
         *  bdaddr_t *dst:
         *    Address of the SDP server device
         */
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern SdpSessionSafeHandle/*"sdp_session_t *"*/sdp_connect(byte[] src,
            byte[] dst, StackConsts.SdpConnectFlags flags);
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern int sdp_close(IntPtr/*"sdp_session_t *"*/session);
        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern int sdp_get_socket(IntPtr/*"sdp_session_t *"*/session);

        /*
         * SDP transaction: functions for asynchronous search.
         */
        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern IntPtr/*"sdp_session_t *"*/sdp_create(int sk, uint32_t flags);
        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern int sdp_get_error(IntPtr/*"sdp_session_t *"*/session);
        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern int sdp_process(IntPtr/*"sdp_session_t *"*/session);
        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern int sdp_set_notify(IntPtr/*"sdp_session_t *"*/session, sdp_callback_t func, IntPtr udata);

        //int sdp_service_search_async(IntPtr/*"sdp_session_t *"*/session, ref sdp_list_t search, uint16_t max_rec_num);
        //int sdp_service_attr_async(IntPtr/*"sdp_session_t *"*/session, uint32_t handle, sdp_attrreq_type_t reqtype, const sdp_list_t *attrid_list);
        //int sdp_service_search_attr_async(IntPtr/*"sdp_session_t *"*/session, const sdp_list_t *search, sdp_attrreq_type_t reqtype, ref sdp_list_t attrid_list);
        //[DllImport(LibraryName, SetLastError = true)]
        //internal static extern int sdp_service_search_attr_async(IntPtr/*"sdp_session_t *"*/session, ref Structs.sdp_list_t search,
        //    StackConsts.sdp_attrreq_type_t reqtype, ref Structs.sdp_list_t attrid_list);

#if false
uint16_t sdp_gen_tid(IntPtr/*"sdp_session_t *"*/session);

/*
 * find all devices in the piconet
 */
int sdp_general_inquiry(inquiry_info *ii, int dev_num, int duration, uint8_t *found);

/* flexible extraction of basic attributes - Jean II */
int sdp_get_int_attr(const sdp_record_t *rec, uint16_t attr, int *value);
int sdp_get_string_attr(const sdp_record_t *rec, uint16_t attr, char *value, int valuelen);

/*
 * Basic sdp data functions
 */
sdp_data_t *sdp_data_alloc(uint8_t dtd, const void *value);
sdp_data_t *sdp_data_alloc_with_length(uint8_t dtd, const void *value, uint32_t length);
void sdp_data_free(sdp_data_t *data);
sdp_data_t *sdp_data_get(const sdp_record_t *rec, uint16_t attr_id);

sdp_data_t *sdp_seq_alloc(void **dtds, void **values, int len);
sdp_data_t *sdp_seq_alloc_with_length(void **dtds, void **values, int *length, int len);
sdp_data_t *sdp_seq_append(sdp_data_t *seq, sdp_data_t *data);

int sdp_attr_add(sdp_record_t *rec, uint16_t attr, sdp_data_t *data);
void sdp_attr_remove(sdp_record_t *rec, uint16_t attr);
void sdp_attr_replace(sdp_record_t *rec, uint16_t attr, sdp_data_t *data);
int sdp_set_uuidseq_attr(sdp_record_t *rec, uint16_t attr, sdp_list_t *seq);
int sdp_get_uuidseq_attr(const sdp_record_t *rec, uint16_t attr, sdp_list_t **seqp);

/*
 * NOTE that none of the functions below will update the SDP server, 
 * unless the {register, update}sdp_record_t() function is invoked.
 * All functions which return an integer value, return 0 on success 
 * or -1 on failure.
 */

/*
 * Create an attribute and add it to the service record's attribute list.
 * This consists of the data type descriptor of the attribute, 
 * the value of the attribute and the attribute identifier.
 */
int sdp_attr_add_new(sdp_record_t *rec, uint16_t attr, uint8_t dtd, const void *p);

/*
 * Set the information attributes of the service record.
 * The set of attributes comprises service name, description 
 * and provider name
 */
void sdp_set_info_attr(sdp_record_t *rec, const char *name, const char *prov, const char *desc);

        //...
        /*
 * Set the access protocols of the record to those specified in proto
 */
int sdp_set_access_protos(ref sdp_record_t rec, ref sdp_list_t proto);

/*
 * Set the additional access protocols of the record to those specified in proto
 */
int sdp_set_add_access_protos(ref sdp_record_t rec, ref sdp_list_t proto);

/*
 * Get protocol port (i.e. PSM for L2CAP, Channel for RFCOMM) 
 */
int sdp_get_proto_port(ref sdp_list_t list, int proto);

/*
 * Get protocol descriptor. 
 */
IntPtr/*"sdp_data_t *"*/sdp_get_proto_desc(ref sdp_list_t list, int proto);

/*
 * Set the LanguageBase attributes to the values specified in list 
 * (a linked list of sdp_lang_attr_t objects, one for each language in 
 * which user-visible attributes are present).
 */
int sdp_set_lang_attr(sdp_record_t *rec, ref sdp_list_t list);

/*
 * Set the ServiceID attribute of a service. 
 */
void sdp_set_service_id(sdp_record_t *rec, uuid_t uuid);

/*
 * Set the GroupID attribute of a service
 */
void sdp_set_group_id(sdp_record_t *rec, uuid_t grouuuid);

#endif
#if false
        /*
 * Set the profile descriptor list attribute of a record.
 * 
 * Each element in the list is an object of type
 * sdp_profile_desc_t which is a definition of the
 * Bluetooth profile that this service conforms to.
 */
int sdp_set_profile_descs(sdp_record_t *rec, const sdp_list_t *desc);

/*
 * Set URL attributes of a record.
 * 
 * ClientExecutableURL: a URL to a client's platform specific (WinCE, 
 * PalmOS) executable code that can be used to access this service.
 * 
 * DocumentationURL: a URL pointing to service documentation
 * 
 * IconURL: a URL to an icon that can be used to represent this service.
 * 
 * Note: pass NULL for any URLs that you don't want to set or remove
 */
void sdp_set_url_attr(sdp_record_t *rec, const char *clientExecURL, const char *docURL, const char *iconURL);

        //..
        /*
 * a service search request. 
 * 
 *  INPUT :
 * 
 *    sdp_list_t *search
 *      list containing elements of the search
 *      pattern. Each entry in the list is a UUID
 *      of the service to be searched
 * 
 *    uint16_t max_rec_num
 *       An integer specifying the maximum number of
 *       entries that the client can handle in the response.
 * 
 *  OUTPUT :
 * 
 *    int return value
 *      0 
 *        The request completed successfully. This does not
 *        mean the requested services were found
 *      -1
 *        The request completed unsuccessfully
 * 
 *    sdp_list_t *rsp_list
 *      This variable is set on a successful return if there are
 *      non-zero service handles. It is a singly linked list of
 *      service record handles (uint16_t)
 */
int sdp_service_search_req(sdp_session_t *session, const sdp_list_t *search, uint16_t max_rec_num, sdp_list_t **rsp_list);

/*
 *  a service attribute request. 
 * 
 *  INPUT :
 * 
 *    uint32_t handle
 *      The handle of the service for which the attribute(s) are
 *      requested
 * 
 *    sdp_attrreq_type_t reqtype
 *      Attribute identifiers are 16 bit unsigned integers specified
 *      in one of 2 ways described below :
 *      SDP_ATTR_REQ_INDIVIDUAL - 16bit individual identifiers
 *         They are the actual attribute identifiers in ascending order
 * 
 *      SDP_ATTR_REQ_RANGE - 32bit identifier range
 *         The high-order 16bits is the start of range
 *         the low-order 16bits are the end of range
 *         0x0000 to 0xFFFF gets all attributes
 * 
 *    sdp_list_t *attrid_list
 *      Singly linked list containing attribute identifiers desired.
 *      Every element is either a uint16_t(attrSpec = SDP_ATTR_REQ_INDIVIDUAL)  
 *      or a uint32_t(attrSpec=SDP_ATTR_REQ_RANGE)
 * 
 *  OUTPUT :
 *    int return value
 *      0 
 *        The request completed successfully. This does not
 *        mean the requested services were found
 *      -1
 *        The request completed unsuccessfully due to a timeout
 */
sdp_record_t *sdp_service_attr_req(sdp_session_t *session, uint32_t handle, sdp_attrreq_type_t reqtype, const sdp_list_t *attrid_list);
#endif

        /*
         *  This is a service search request combined with the service
         *  attribute request. First a service class match is done and
         *  for matching service, requested attributes are extracted
         * 
         *  INPUT :
         * 
         *    sdp_list_t *search
         *      Singly linked list containing elements of the search
         *      pattern. Each entry in the list is a UUID(DataTypeSDP_UUID16)
         *      of the service to be searched
         * 
         *    AttributeSpecification attrSpec
         *      Attribute identifiers are 16 bit unsigned integers specified
         *      in one of 2 ways described below :
         *      SDP_ATTR_REQ_INDIVIDUAL - 16bit individual identifiers
         *         They are the actual attribute identifiers in ascending order
         * 
         *      SDP_ATTR_REQ_RANGE - 32bit identifier range
         *         The high-order 16bits is the start of range
         *         the low-order 16bits are the end of range
         *         0x0000 to 0xFFFF gets all attributes
         * 
         *    sdp_list_t *attrid_list
         *      Singly linked list containing attribute identifiers desired.
         *      Every element is either a uint16_t(attrSpec = SDP_ATTR_REQ_INDIVIDUAL)  
         *      or a uint32_t(attrSpec=SDP_ATTR_REQ_RANGE)
         * 
         *  OUTPUT :
         *    int return value
         *      0 
         *        The request completed successfully. This does not
         *        mean the requested services were found
         *      -1
         *        The request completed unsuccessfully due to a timeout
         * 
         *    sdp_list_t *rsp_list
         *      This variable is set on a successful return to point to
         *      service(s) found. Each element of this list is of type
         *      sdp_record_t *.
         */
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern BluezError sdp_service_search_attr_req(SdpSessionSafeHandle session,
            IntPtr search,
            StackConsts.sdp_attrreq_type_t reqtype, IntPtr attrid_list,
            out IntPtr /*"sdp_list_t**"*/ ppRsp_list);

        //----
        [DllImport(LibraryName, SetLastError = true)]
        internal static extern IntPtr/*"sdp_record_t *"*/sdp_extract_pdu(byte[] pdata, int len, out int scanned);

        [DllImport(LibraryName, SetLastError = true)]
        internal static extern BluezError sdp_record_register(NativeMethods.SdpSessionSafeHandle/*"sdp_session_t*"*/ session,
            IntPtr/*"sdp_record_t*"*/ rec, byte flags);

        #endregion

        //--------
        internal class SdpSessionSafeHandle : SafeHandle
        {
            public SdpSessionSafeHandle()
                : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid
            {
                get { return handle == IntPtr.Zero; }
            }

            protected override bool ReleaseHandle()
            {
                int ret = NativeMethods.sdp_close(this.handle);
                bool success = ret >= 0;
                return success;
            }
        }//class

    }
}
