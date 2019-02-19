using GarasERP;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GarasSales.Sales.Client
{
    public partial class EditClient : System.Web.UI.Page
    {
        protected string myType = "";
        string selectedRadio = String.Empty;
        string selectedName = String.Empty;
        string selectedConsultantName = String.Empty;
        //DataTable dt = new DataTable();
        static string key = "SalesGarasPass";
        private long UserID
        {
            get
            {
                if (Session["UserID"] == null)
                {
                    return 0;
                }
                else
                {
                    string id = Session["UserID"].ToString();

                    return long.Parse(GarasERP.Encrypt_Decrypt.Decrypt(id, key));
                }
            }
            set
            {
                Session["UserID"] = value;
            }
        }

        private DataTable VS_Speciality
        {
            get
            {
                if (ViewState["VS_Speciality"] == null)
                {
                    return new DataTable();
                }
                return (DataTable)ViewState["VS_Speciality"];
            }
            set
            {
                ViewState["VS_Speciality"] = value;
            }
        }

        private DataTable VS_Country
        {
            get
            {
                if (ViewState["VS_Country"] == null)
                {
                    return new DataTable();
                }
                return (DataTable)ViewState["VS_Country"];
            }
            set
            {
                ViewState["VS_Country"] = value;
            }
        }

        private DataTable VS_Consultant
        {
            get
            {
                if (ViewState["VS_Consultant"] == null)
                {
                    return new DataTable();
                }
                return (DataTable)ViewState["VS_Consultant"];
            }
            set
            {
                ViewState["VS_Consultant"] = value;
            }
        }

        private DataRow VS_Consultant_Footer
        {
            get
            {
                if (ViewState["VS_Consultant_Footer"] == null)
                {
                    return null;
                }
                return (DataRow)ViewState["VS_Consultant_Footer"];
            }
            set
            {
                ViewState["VS_Consultant_Footer"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    if (Page.Request.QueryString.Count > 0 && Page.Request.QueryString["CID"] != null)
                    {
                        string ID = Encrypt_Decrypt.Decrypt(Page.Request.QueryString["CID"].ToString(), key);
                        long CID = 0;
                        if (long.TryParse(ID, out CID) && CID > 0)
                        {
                            
                            if (Common.CheckUserInRole(UserID, 1))
                            {
                                int BranchID = Common.GetUserBranchID(UserID);
                                V_Client_Useer client = new V_Client_Useer();
                                client.Where.ID.Value = CID;
                                client.Where.BranchID.Value = BranchID;
                                if (client.Query.Load())
                                {
                                    if (client.DefaultView != null && client.DefaultView.Count > 0)
                                    {
                                        newClientPage.Visible = true;
                                        noAccess.Visible = false;
                                        BindAllDDLs();
                                        Get_Client_Details(client);
                                        LoadNameBasedOnType();
                                        LoadConsultantDivIfAvail();
                                        //LoadConsultantBasedOnType();

                                    }
                                    else
                                    {
                                        LBL_MSG.Visible = true;
                                        LBL_MSG.Text = "Please Select a correct Client";
                                    }
                                }
                                else
                                {
                                    LBL_MSG.Visible = true;
                                    LBL_MSG.Text = "Please Select a correct Client";
                                }
                            }
                            else
                            {
                                newClientPage.Visible = false;
                                noAccess.Visible = true;
                                LBL_MSG.Visible = true;
                                LBL_MSG.Text = "You Don't have permission to edit this client";
                            }

                           
                              
                            
                        }
                        else
                        {
                            LBL_MSG.Visible = true;
                            LBL_MSG.Text = "Please Select a correct Client";
                        }
                    }
                    else
                    {
                        LBL_MSG.Visible = true;
                        LBL_MSG.Text = "Please Select a correct Client";
                    }
                }
                else
                {
                    rebuildVS_Consultant(false);
                    LoadNameBasedOnType();
                    LoadConsultantDivIfAvail();
                    //LoadConsultantBasedOnType();
                }

               


            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "Page_Load:" + ex.Message;
            }
        }

        private void Get_Client_Details(V_Client_Useer client)
        {

            try
            {
                

                myType = client.Type;
                //DataView dt = client.DefaultView;
                //  LBL_ClientName.Text = client.Name.ToString().Trim();
                //if (!string.IsNullOrEmpty(client.s_FirstContractDate))
                //    LBL_ClientDuration.Text = client.FirstContractDate.ToString("yyyy");

                //this.Page.Title = client.Name.ToString() + "'s Profile";
                switch (myType)
                {
                    case "Small Company (One Branch)":
                        {
                            lblCompanyName.Visible = true;
                            tbxCompanyName.Visible = true;
                            tbxCompanyName.Text = client.Name;

                            lblGroupName.Visible = false;
                            tbxGroupName.Visible = false;
                            RFV_tbxGroupName.Enabled = false;

                            lblBranch.Visible = false;
                            tbxBranch.Visible = false;
                            RFV_tbxBranch.Enabled = false;
                            selectedRadio = "Small Company (One Branch)";
                            selectedName = tbxCompanyName.Text;
                            rbSmall.Checked = true;
                        }
                        break;
                    case "Big Company (Multiple Branches)":
                        {
                            lblCompanyName.Visible = true;
                            tbxCompanyName.Visible = true;

                            lblBranch.Visible = true;
                            tbxBranch.Visible = true;
                            tbxBranch.Text = client.BranchName;
                            RFV_tbxBranch.Enabled = true;
                            lblGroupName.Visible = false;
                            tbxGroupName.Visible = false;
                            RFV_tbxGroupName.Enabled = false;
                            selectedRadio = "Big Company (Multiple Branches)";
                            selectedName = tbxCompanyName.Text;

                            
                            tbxCompanyName.Text = client.Name;
                           
                            tbxBranch.Text = client.BranchName;
                           // tbxGroupName.Text = client.GroupName;
                            rbBig.Checked = true;

                        }
                        break;

                    case "Group of Companies":
                        {
                            lblCompanyName.Visible = true;
                            tbxCompanyName.Visible = true;
                            tbxCompanyName.Text = client.Name;
                            lblBranch.Visible = true;
                            tbxBranch.Visible = true;
                            tbxBranch.Text = client.BranchName;
                            RFV_tbxBranch.Enabled = true;
                            lblGroupName.Visible = true;
                            tbxGroupName.Visible = true;
                            tbxGroupName.Text = client.GroupName;
                            RFV_tbxGroupName.Enabled = true;
                            selectedRadio = "Group of Companies";
                            selectedName = tbxCompanyName.Text;
                            rbCompanies.Checked = true;
                        }
                        break;

                    case "Individual":
                        {
                            lblCompanyName.Visible = true;
                            tbxCompanyName.Visible = true;
                            tbxCompanyName.Text = client.Name;
                            lblGroupName.Visible = false;
                            tbxGroupName.Visible = false;
                            RFV_tbxGroupName.Enabled = false;
                            lblBranch.Visible = false;
                            tbxBranch.Visible = false;
                            RFV_tbxBranch.Enabled = true;
                            selectedRadio = "Individual";
                            selectedName = lblCompanyName.Text;
                            rbIndividual.Checked = true;
                        }
                        break;
                }



                if (client.s_SupportedByCompany != "")
                {
                    if (client.SupportedByCompany)
                    {
                        CHB_SupportedByCompany.Checked = true;
                        DDL_Supported.Visible = true;
                        DDL_Supported.SelectedValue = client.SupportedBy;
                    }
                }



                LoadClientAddresses(client.ID);
                tbxEmail.Text = client.Email;
                tbxWebsite.Text = client.WebSite;
                loadClientPhones(client.ID);
                loadClientMobile(client.ID);
                loadClientFAX(client.ID);
                LoadClientContacts(client.ID);
                LoadClientSpeciality(client.ID);


                switch (client.ConsultantType)
                {
                    case "":
                        {
                            rptrConsaltant.Visible = false;
                            chbxAvailable.Checked = false;
                            //consultantFirstDiv.Visible = false;
                            //consultantSecondDiv.Visible = false;
                            //rbConsultantNA.Checked = true;
                            //rbConsultantCompany.Checked = false;
                        }
                        break;

                    case "N/A":
                        {
                            rptrConsaltant.Visible = false;
                            chbxAvailable.Checked = false;
                            //consultantFirstDiv.Visible = false;
                            //consultantSecondDiv.Visible = false;
                            //rbConsultantNA.Checked = true;
                            //rbConsultantCompany.Checked = false;
                        }
                        break;
                    case "Has Consultant":
                        rptrConsaltant.Visible = true;
                        chbxAvailable.Checked = true;
                        LoadConsaltant(client.ID);
                        break;
                    //case "Company / Office":
                    //    {
                    //        consultantFirstDiv.Visible = true;
                    //        consultantSecondDiv.Visible = true;
                    //        //consultantContacts.Visible = true;

                    //        lblConsultantOffice.Visible = true;
                    //        tbxConsultantOffice.Visible = true;

                    //        lblConsultantName.Visible = true;
                    //        tbxConsultantName.Visible = true;
                    //        rbConsultantCompany.Checked = true;
                    //        rbConsultantNA.Checked = false;
                    //        LoadConsaltant(client.ID);

                    //    }
                    //    break;

                  

                    //case "Individual":
                    //    {
                    //        consultantFirstDiv.Visible = true;
                    //        consultantSecondDiv.Visible = true;
                    //        //consultantContacts.Visible = true;

                    //        lblConsultantOffice.Visible = false;
                    //        tbxConsultantOffice.Visible = false;

                    //        lblConsultantName.Visible = true;
                    //        tbxConsultantName.Visible = true;
                    //        rbConsultantCompany.Checked = false;
                    //        rbConsultantNA.Checked = false;
                    //        rbConsultantIndiv.Checked = true;
                    //        LoadConsaltant(client.ID);
                    //    }
                    //    break;
                }





               
                
                
                
                ddlFollowUpPeriod.SelectedValue = client.s_FollowUpPeriod;
                ddlAssignedTo.SelectedValue = client.s_SalesPersonID;
                LoadAttachment(client.ID);
                tbxNote.Text = client.Note;

            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "Get_Client:" + ex.Message;
            }

        }

        private void LoadAttachment(long iD)
        {
            try
            {
                DataTable FilesDT = new System.Data.DataTable();
                FilesDT.Columns.Add("FileName");
                FilesDT.Columns.Add("AttachmentPath");
                FilesDT.Columns.Add("Category");

                ClientAttachment attach = new ClientAttachment();
                attach.Where.ClientID.Value = iD;
                attach.Query.AddOrderBy(ClientAttachment.ColumnNames.Type, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
                if (attach.Query.Load())
                {

                    do
                    {
                        DataRow dr = FilesDT.NewRow();
                        dr["FileName"] = attach.FileName;
                        dr["AttachmentPath"] = attach.AttachmentPath;
                        dr["Category"] = attach.Type;



                        FilesDT.Rows.Add(dr);
                    }
                    while (attach.MoveNext());

                    AttachmentsBC.BindAttachments(FilesDT.DefaultView);
                }

            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadAttachment:" + ex.Message;
            }
        }


        //Show and hide textboxes & labels based on type chosen by user
        protected void LoadNameBasedOnType()
        {
            if (rbBig.Checked == true)
            {
                lblCompanyName.Visible = true;
                tbxCompanyName.Visible = true;

                lblBranch.Visible = true;
                tbxBranch.Visible = true;
                RFV_tbxBranch.Enabled = true;

                lblGroupName.Visible = false;
                tbxGroupName.Visible = false;
                RFV_tbxGroupName.Enabled = false;

                selectedRadio = "Big Company (Multiple Branches)";
                selectedName = tbxCompanyName.Text;
            }

            else if (rbCompanies.Checked == true)
            {
                lblGroupName.Visible = true;
                tbxGroupName.Visible = true;
                RFV_tbxGroupName.Enabled = true;

                lblCompanyName.Visible = true;
                tbxCompanyName.Visible = true;


                lblBranch.Visible = false;
                tbxBranch.Visible = false;
                RFV_tbxBranch.Enabled = false;

                selectedRadio = "Group of Companies";
                selectedName = tbxCompanyName.Text;
            }
            else if (rbSmall.Checked == true)
            {
                lblCompanyName.Visible = true;
                tbxCompanyName.Visible = true;

                lblGroupName.Visible = false;
                tbxGroupName.Visible = false;
                RFV_tbxGroupName.Enabled = false;

                lblBranch.Visible = false;
                tbxBranch.Visible = false;
                RFV_tbxBranch.Enabled = true;

                selectedRadio = "Small Company (One Branch)";
                selectedName = tbxCompanyName.Text;
            }
            else if (rbIndividual.Checked == true)
            {
                lblCompanyName.Visible = true;
                tbxCompanyName.Visible = true;

                lblGroupName.Visible = false;
                tbxGroupName.Visible = false;
                RFV_tbxGroupName.Enabled = false;


                lblBranch.Visible = false;
                tbxBranch.Visible = false;
                RFV_tbxBranch.Enabled = false;

                selectedRadio = "Individual";
                selectedName = lblCompanyName.Text;
            }
            else
            {
                lblGroupName.Visible = false;
                tbxGroupName.Visible = false;
                lblCompanyName.Visible = false;
                tbxCompanyName.Visible = false;
                lblBranch.Visible = false;
                tbxBranch.Visible = false;

            }
        }


        private void LoadClientAddresses(long iD)
        {
            try
            {
               

                V_ClientAddress Address = new V_ClientAddress();
                Address.Where.ClientID.Value = iD;
                Address.Where.Active.Value = true;
                int count = 1;
                if (Address.Query.Load())
                {
                    do
                    {
                       if(count==1)
                        {
                            HDN_AddressID.Value = Address.s_ID;
                            ddlCountry.SelectedValue = Address.s_CountryID;
                            BindGovDDL();
                            ddlGovernate.SelectedValue = Address.s_GovernorateID;
                            LoadArea();
                            if (Address.s_AreaID != "")
                                DDL_Area.SelectedValue = Address.s_AreaID;
                            tbxStreet.Text = Address.Address;
                            tbxBuilding.Text = Address.BuildingNumber;
                            tbxFloor.Text = Address.Floor;
                            tbxDesc.Text = Address.Description;
                        }
                        if (count == 2)
                        {
                            address2.Visible = true;
                            RFV_ddlCountry2.Enabled = true;
                            RFV_ddlGovernate2.Enabled = true;
                            pAddNewAddress1.Visible = false;

                            HDN_AddressID2.Value = Address.s_ID;
                            ddlCountry2.SelectedValue = Address.s_CountryID;
                            BindGovDDL2();
                            ddlGovernate2.SelectedValue = Address.s_GovernorateID;
                            LoadArea2();
                            if (Address.s_AreaID != "")
                                DDL_Area2.SelectedValue = Address.s_AreaID;
                            tbxStreet2.Text = Address.Address;
                            tbxBuilding2.Text = Address.BuildingNumber;
                            tbxFloor2.Text = Address.Floor;
                            tbxDesc2.Text = Address.Description;

                        }
                        if (count == 3)
                        {
                            address3.Visible = true;
                            RFV_ddlCountry3.Enabled = true;
                            RFV_ddlGovernate3.Enabled = true;
                            pAddNewAddress2.Visible = false;
                            HDN_AddressID3.Value = Address.s_ID;

                            ddlCountry3.SelectedValue = Address.s_CountryID;
                            BindGovDDL3();
                            ddlGovernate3.SelectedValue = Address.s_GovernorateID;
                            LoadArea3();
                            if (Address.s_AreaID != "")
                                DDL_Area3.SelectedValue = Address.s_AreaID;
                            tbxStreet3.Text = Address.Address;
                            tbxBuilding3.Text = Address.BuildingNumber;
                            tbxFloor3.Text = Address.Floor;
                            tbxDesc3.Text = Address.Description;
                        }
                        count++;
                    }
                    while (Address.MoveNext());
                }

              




            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadClientAddresses:" + ex.Message;
            }
        }

        private void loadClientPhones(long iD)
        {
            try
            {
                ClientPhone phone = new ClientPhone();
                phone.Where.ClientID.Value = iD;
                phone.Where.Active.Value = true;
                if (phone.Query.Load())
                {
                    int count = 1;
                    do
                    {
                        if(count==1)
                        {
                            HDN_PhoneID.Value = phone.s_ID;
                            tbxPhone.Text = phone.Phone;
                        }
                        if (count == 2)
                        {
                            newPhone.Visible = true;
                            btnAddNewPhone.Visible = false;
                            RegularExpressionValidator6.Enabled = true;
                            HDN_PhoneID2.Value = phone.s_ID;
                            tbxPhone2.Text = phone.Phone;
                        }
                        if (count == 3)
                        {
                            newPhone3.Visible = true;
                            btnAddNewPhone2.Visible = false;
                            RegularExpressionValidator7.Enabled = true;
                            HDN_PhoneID3.Value = phone.s_ID;
                            tbxPhone3.Text = phone.Phone;
                        }

                        count++;
                    } while (phone.MoveNext());
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "loadClientPhones:" + ex.Message;
            }
        }

        private void loadClientMobile(long iD)
        {
            try
            {
                ClientMobile mobile = new ClientMobile();
                mobile.Where.ClientID.Value = iD;
                mobile.Where.Active.Value = true;
                if (mobile.Query.Load())
                {
                    int count = 1;
                    do
                    {
                        if (count == 1)
                        {
                            HDN_MobileID.Value = mobile.s_ID;
                            tbxMobile.Text = mobile.Mobile;
                        }
                        if (count == 2)
                        {
                            newMobile2.Visible = true;
                            btnAddNewMobile.Visible = false;
                            RegularExpressionValidator8.Enabled = true;
                            HDN_MobileID2.Value = mobile.s_ID;
                            tbxMobile2.Text = mobile.Mobile;
                        }
                        if (count == 3)
                        {
                            newMobile3.Visible = true;
                            btnAddNewMobile3.Visible = false;
                            RegularExpressionValidator9.Enabled = true;
                            HDN_MobileID3.Value = mobile.s_ID;
                            tbxMobile3.Text = mobile.Mobile;
                        }

                        count++;
                    } while (mobile.MoveNext());
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "loadClientMobile:" + ex.Message;
            }
        }

        private void loadClientFAX(long iD)
        {
            try
            {
                ClientFax fax = new ClientFax();
                fax.Where.ClientID.Value = iD;
                fax.Where.Active.Value = true;
                if (fax.Query.Load())
                {
                    int count = 1;
                    do
                    {
                        if (count == 1)
                        {
                            HDN_FAXID.Value = fax.s_ID;
                            tbxFax.Text = fax.Fax;
                        }
                        if (count == 2)
                        {
                            newFax2.Visible = true;
                            btnAddNewFax.Visible = false;
                            RegularExpressionValidator10.Enabled = true;
                            HDN_FaxID2.Value = fax.s_ID;
                            tbxFax2.Text = fax.Fax;
                        }
                        if (count == 3)
                        {
                            newFax3.Visible = true;
                            btnAddNewFax3.Visible = false;
                            RegularExpressionValidator11.Enabled = true;
                            HDN_FaxeID3.Value = fax.s_ID;
                            tbxFax3.Text = fax.Fax;
                        }

                        count++;
                    } while (fax.MoveNext());
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "loadClientFAX:" + ex.Message;
            }
        }

        private void LoadClientContacts(long iD)
        {
            try
            {
                ClientContactPerson contact = new ClientContactPerson();
                contact.Where.ClientID.Value = iD;
                contact.Where.Active.Value = true;
                if (contact.Query.Load())
                {
                    int count = 1;
                    do
                    {
                        if (count == 1)
                        {
                            HDN_ContactID.Value = contact.s_ID;
                            tbxContactPerson.Text = contact.Name;
                            tbxContactTitle.Text = contact.Title;
                            tbxContactEmail.Text = contact.Email;
                            tbxContactMobile.Text = contact.Mobile;
                            tbxContactLocation.Text = contact.Location;
                        }
                        if (count == 2)
                        {
                            HDN_ContactID1.Value = contact.s_ID;
                            newContact2.Visible = true;
                            pAddNewContact.Visible = false;
                            RequiredFieldValidator3.Enabled = true;
                            RequiredFieldValidator4.Enabled = true;
                            RequiredFieldValidator5.Enabled = true;
                            tbxContactName2.Text = contact.Name;
                            tbxContactTitle2.Text = contact.Title;
                            tbxContactEmail2.Text = contact.Email;
                            tbxContactMobile2.Text = contact.Mobile;
                            tbxContactLocation2.Text = contact.Location;
                        }
                        if (count == 3)
                        {
                            HDN_ConractID2.Value = contact.s_ID;
                            newContact3.Visible = true;
                            pAddNewContact2.Visible = false;
                            RequiredFieldValidator6.Enabled = true;
                            RequiredFieldValidator7.Enabled = true;
                            RequiredFieldValidator8.Enabled = true;
                            tbxContactName3.Text = contact.Name;
                            tbxContactTitle3.Text = contact.Title;
                            tbxContactEmail3.Text = contact.Email;
                            tbxContactMobile3.Text = contact.Mobile;
                            tbxContactLocation3.Text = contact.Location;
                        }

                        count++;
                    } while (contact.MoveNext());
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadClientContacts:" + ex.Message;
            }
        }


        private void LoadClientSpeciality(long iD)
        {
            try
            {
                V_ClientSpeciality specialty = new V_ClientSpeciality();
                specialty.Where.ClientID.Value = iD;
                specialty.Where.Active.Value = true;
                if (specialty.Query.Load())
                {
                    int count = 1;
                    do
                    {
                        if (count == 1)
                        {
                            HDN_SpecialityID.Value = specialty.s_ID;
                            ddlSpeciality.SelectedValue = specialty.s_SpecialityID;
                        }
                        if (count == 2)
                        {
                            speciality2.Visible = true;
                            btnAddSpeciality.Visible = false;
                            HDN_SpecialityID2.Value = specialty.s_ID;
                            ddlSpeciality2.SelectedValue = specialty.s_SpecialityID;
                        }
                        if (count == 3)
                        {
                            speciality3.Visible = true;
                            btnAddSpeciality3.Visible = false;
                            HDN_SpecialityID3.Value = specialty.s_ID;
                            ddlSpeciality3.SelectedValue = specialty.s_SpecialityID;
                        }

                        if (count == 4)
                        {
                            speciality4.Visible = true;
                            btnAddSpeciality4.Visible = false;
                            HDN_SpecialityID4.Value = specialty.s_ID;
                            ddlSpeciality4.SelectedValue = specialty.s_SpecialityID;
                        }

                        count++;
                    } while (specialty.MoveNext());
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadClientSpeciality:" + ex.Message;
            }
        }

        //protected void LoadConsultantBasedOnType()
        //{
        //    if (rbConsultantNA.Checked == true)
        //    {
        //        consultantFirstDiv.Visible = false;
        //        consultantSecondDiv.Visible = false;
        //        //consultantContacts.Visible = false;
        //    }
        //    else if (rbConsultantCompany.Checked == true)
        //    {
        //        consultantFirstDiv.Visible = true;
        //        consultantSecondDiv.Visible = true;
        //        //consultantContacts.Visible = true;

        //        lblConsultantOffice.Visible = true;
        //        tbxConsultantOffice.Visible = true;

        //        lblConsultantName.Visible = true;
        //        tbxConsultantName.Visible = true;
        //    }
        //    else if (rbConsultantIndiv.Checked == true)
        //    {
        //        consultantFirstDiv.Visible = true;
        //        consultantSecondDiv.Visible = true;
        //        //consultantContacts.Visible = true;

        //        lblConsultantOffice.Visible = false;
        //        tbxConsultantOffice.Visible = false;

        //        lblConsultantName.Visible = true;
        //        tbxConsultantName.Visible = true;
        //    }

        //}

        protected void LoadConsultantDivIfAvail()
        {
            if (chbxAvailable.Checked != true)
            {
                rptrConsaltant.Visible = false;
            }
            else
            {
                rptrConsaltant.Visible = true;
            }
            updatePanelConsultantType.Update();
        }

        private void LoadConsaltant(long iD)
        {
            try
            {
                


                ClientConsultant Const = new ClientConsultant();
                Const.Where.ClientID.Value = iD;
                Const.Where.Active.Value = true;
                if (Const.Query.Load())
                {
                  if(Const.DefaultView!=null && Const.DefaultView.Count>0)
                    {
                        Const.Rewind(); //move to first record
                        DataTable dt = initializeConsultDT();
                        do
                        {
                            DataRow dr = dt.NewRow();
                            //HDN_ConsultantID.Value = Const.s_ID;
                            //tbxConsultantOffice.Text = Const.Company;
                            //tbxConsultantName.Text = Const.ConsultantName;
                            //DDl_For.SelectedValue = Const.ConsultantFor;
                            dr["Consult_ID"] = Const.s_ID;
                            dr["ConsltName"] = Const.ConsultantName;
                            dr["ConsltOffice"] = Const.Company;
                            dr["ConsltFor"] = Const.ConsultantFor;                            
                            LoadConsaltantSpeciality(Const.ID,dr);
                            LoadConsaltantEmails(Const.ID,dr);
                            LoadConsaltantPhone(Const.ID,dr);
                            LoadConsaltantMobile(Const.ID,dr);
                            LoadConsaltantFax(Const.ID,dr);
                            LoadConsaltantAddress(Const.ID,dr);
                            dt.Rows.Add(dr);
                        } while (Const.MoveNext());
                        dt.AcceptChanges();
                        VS_Consultant = dt;
                    }
                }

               
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadConsaltant:" + ex.Message;
            }
        }

        private DataTable initializeConsultDT()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Consult_ID", typeof(string));
            dt.Columns.Add("ConsltName", typeof(string));
            dt.Columns.Add("ConsltOffice", typeof(string));
            dt.Columns.Add("ConsltFor", typeof(string));
            dt.Columns.Add("ConsltAddress_ID", typeof(string));
            dt.Columns.Add("ConsltCountry", typeof(string));
            dt.Columns.Add("ConsltGvrnrt", typeof(string));
            dt.Columns.Add("ConsltStreet", typeof(string));
            dt.Columns.Add("ConsltBuilding", typeof(string));
            dt.Columns.Add("ConsltFloor", typeof(string));
            dt.Columns.Add("ConsltDesc", typeof(string));
            dt.Columns.Add("ConsltEmail_ID", typeof(string));
            dt.Columns.Add("ConsltEmail", typeof(string));
            dt.Columns.Add("ConsltEmail1_ID", typeof(string));
            dt.Columns.Add("ConsltEmail1", typeof(string));
            dt.Columns.Add("ConsltEmail2_ID", typeof(string));
            dt.Columns.Add("ConsltEmail2", typeof(string));
            dt.Columns.Add("ConsltPhone_ID", typeof(string));
            dt.Columns.Add("ConsltPhone", typeof(string));
            dt.Columns.Add("ConsltPhone1_ID", typeof(string));
            dt.Columns.Add("ConsltPhone1", typeof(string));
            dt.Columns.Add("ConsltPhone2_ID", typeof(string));
            dt.Columns.Add("ConsltPhone2", typeof(string));
            dt.Columns.Add("ConsltMobile_ID", typeof(string));
            dt.Columns.Add("ConsltMobile", typeof(string));
            dt.Columns.Add("ConsltMobile1_ID", typeof(string));
            dt.Columns.Add("ConsltMobile1", typeof(string));
            dt.Columns.Add("ConsltMobile2_ID", typeof(string));
            dt.Columns.Add("ConsltMobile2", typeof(string));
            dt.Columns.Add("ConsltFax_ID", typeof(string));
            dt.Columns.Add("ConsltFax", typeof(string));
            dt.Columns.Add("ConsltFax1_ID", typeof(string));
            dt.Columns.Add("ConsltFax1", typeof(string));
            dt.Columns.Add("ConsltFax2_ID", typeof(string));
            dt.Columns.Add("ConsltFax2", typeof(string));
            dt.Columns.Add("ConsltSpecial1_ID", typeof(string));
            dt.Columns.Add("Special1", typeof(string));
            dt.Columns.Add("ConsltSpecial2_ID", typeof(string));
            dt.Columns.Add("Special2", typeof(string));
            dt.Columns.Add("ConsltSpecial3_ID", typeof(string));
            dt.Columns.Add("Special3", typeof(string));
            dt.AcceptChanges();
            return dt;
        }

        private void LoadConsaltantAddress(long iD, DataRow dr)
        {
            try
            {
                ClientConsultantAddress Address = new ClientConsultantAddress();
                Address.Where.ConsultantID.Value = iD;
                Address.Where.Active.Value = true;
                if (Address.Query.Load())
                {
                    if (Address.DefaultView != null && Address.DefaultView.Count > 0)
                    {
                        //HDN_ddlConsultantAddressID.Value = Address.s_ID;
                        //ddlConsultantCountry.SelectedValue = Address.s_CountryID;
                        //ddlConsultantGovernorate.SelectedValue = Address.s_GovernorateID;
                        //tbxConsultantStreet.Text = Address.Address;
                        //tbxConsultantBuilding.Text = Address.BuildingNumber;
                        //tbxConsultantFloor.Text = Address.Floor;
                        //tbxConsultantDescription.Text = Address.Description;
                        dr["ConsltAddress_ID"] = Address.s_ID;
                        dr["ConsltCountry"] = Address.s_CountryID;
                        dr["ConsltGvrnrt"] = Address.s_GovernorateID;
                        dr["ConsltStreet"] = Address.Address;
                        dr["ConsltBuilding"] = Address.BuildingNumber;
                        dr["ConsltFloor"] = Address.Floor;
                        dr["ConsltDesc"] = Address.Description;
                    }
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadConsaltantAddress:" + ex.Message;
            }
        }

        private void LoadConsaltantFax(long iD, DataRow dr)
        {
            try
            {
                ClientConsultantFax Fax = new  ClientConsultantFax();
                Fax.Where.ConsultantID.Value = iD;
                Fax.Where.Active.Value = true;
                if (Fax.Query.Load())
                {
                    int count = 1;
                    do
                    {
                        if (count == 1)
                        {
                            //HDN_ConsultantFaxID.Value = Fax.s_ID;
                            //tbxConsultantFax.Text = Fax.Fax;
                            dr["ConsltFax_ID"] = Fax.s_ID;
                            dr["ConsltFax"] = Fax.Fax;
                        }
                        if (count == 2)
                        {
                            //HDN_ConsultantFaxID2.Value = Fax.s_ID;
                            //tbxConsultantFax2.Text = Fax.Fax;
                            dr["ConsltFax1_ID"] = Fax.s_ID;
                            dr["ConsltFax1"] = Fax.Fax;
                        }
                        if (count == 3)
                        {
                            //HDN_ConsultantFaxID3.Value = Fax.s_ID;
                            //tbxConsultantFax3.Text = Fax.Fax;
                            dr["ConsltFax2_ID"] = Fax.s_ID;
                            dr["ConsltFax2"] = Fax.Fax;
                        }

                        count++;
                    } while (Fax.MoveNext());
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadConsaltantFax:" + ex.Message;
            }
        }

        private void LoadConsaltantMobile(long iD, DataRow dr)
        {
            try
            {
                ClientConsultantMobile Mobile = new ClientConsultantMobile();
                Mobile.Where.ConsultantID.Value = iD;
                Mobile.Where.Active.Value = true;
                if (Mobile.Query.Load())
                {
                    int count = 1;
                    do
                    {
                        if (count == 1)
                        {
                            //HDN_ConsultantMobileID.Value = Mobile.s_ID;
                            //tbxConsultantMobile.Text = Mobile.Mobile;
                            dr["ConsltMobile_ID"] = Mobile.s_ID;
                            dr["ConsltMobile"] = Mobile.Mobile;
                        }
                        if (count == 2)
                        {
                            //newConsultantMobile2.Visible = true;
                            //btnAddNewConsultantMobile.Visible = false;
                            //HDN_ConsultantMobileID2.Value = Mobile.s_ID;
                            //tbxConsultantMobile2.Text = Mobile.Mobile;
                            dr["ConsltMobile1_ID"] = Mobile.s_ID;
                            dr["ConsltMobile1"] = Mobile.Mobile;
                        }
                        if (count == 3)
                        {
                            //newConsultantMobile3.Visible = true;
                            //btnAddNewConsultantMobile3.Visible = false;
                            //HDN_ConsultantMobileID3.Value = Mobile.s_ID;
                            //tbxConsultantMobile3.Text = Mobile.Mobile;
                            dr["ConsltMobile2_ID"] = Mobile.s_ID;
                            dr["ConsltMobile2"] = Mobile.Mobile;
                        }

                        count++;
                    } while (Mobile.MoveNext());
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadConsaltantPhone:" + ex.Message;
            }
        }

        private void LoadConsaltantPhone(long iD, DataRow dr)
        {
            try
            {
                ClientConsultantPhone Phone = new ClientConsultantPhone();
                Phone.Where.ConsultantID.Value = iD;
                Phone.Where.Active.Value = true;
                if (Phone.Query.Load())
                {
                    int count = 1;
                    do
                    {
                        if (count == 1)
                        {
                            //HDN_ConsultantPhoneID.Value = Phone.s_ID;
                            //tbxConsultantPhone.Text = Phone.Phone;
                            dr["ConsltPhone_ID"] = Phone.s_ID;
                            dr["ConsltPhone"] = Phone.Phone;
                        }
                        if (count == 2)
                        {
                            //newConsultantPhone2.Visible = true;
                            //btnAddNewConsultantPhone.Visible = false;
                            //HDN_ConsultantPhoneID2.Value = Phone.s_ID;
                            //tbxConsultantPhone2.Text = Phone.Phone;
                            dr["ConsltPhone1_ID"] = Phone.s_ID;
                            dr["ConsltPhone1"] = Phone.Phone;
                        }
                        if (count == 3)
                        {
                            //newConsultantPhone3.Visible = true;
                            //btnAddNewConsultantPhone2.Visible = false;
                            //HDN_ConsultantPhoneID3.Value = Phone.s_ID;
                            //tbxConsultantPhone3.Text = Phone.Phone;
                            dr["ConsltPhone2_ID"] = Phone.s_ID;
                            dr["ConsltPhone2"] = Phone.Phone;
                        }

                        count++;
                    } while (Phone.MoveNext());
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadConsaltantPhone:" + ex.Message;
            }
        }

        private void LoadConsaltantEmails(long iD, DataRow dr)
        {
            try
            {
                ClientConsultantEmail Email = new ClientConsultantEmail();
                Email.Where.ConsultantID.Value = iD;
                Email.Where.Active.Value = true;
                if (Email.Query.Load())
                {
                    int count = 1;
                    do
                    {
                        if (count == 1)
                        {
                            //HDN_ConsultantEmailID.Value = Email.s_ID;
                            //tbxConsultantEmail.Text = Email.Email;
                            dr["ConsltEmail_ID"] = Email.s_ID;
                            dr["ConsltEmail"] = Email.Email;
                        }
                        if (count == 2)
                        {
                            //newConsultantEmail2.Visible = true;
                            //btnAddConsultantEmail.Visible = false;
                            //HDN_ConsultantEmailID2.Value = Email.s_ID;
                            //tbxConsultantEmail2.Text = Email.Email;
                            dr["ConsltEmail1_ID"] = Email.s_ID;
                            dr["ConsltEmail1"] = Email.Email;
                        }
                        if (count == 3)
                        {
                            //newConsultantEmail3.Visible = true;
                            //btnAddConsultantEmail3.Visible = false;
                            //HDN_ConsultantEmailID2.Value = Email.s_ID;
                            //tbxConsultantEmail3.Text = Email.Email;
                            dr["ConsltEmail2_ID"] = Email.s_ID;
                            dr["ConsltEmail2"] = Email.Email;
                        }

                        count++;
                    } while (Email.MoveNext());
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadConsaltantEmails:" + ex.Message;
            }
        }

        private void LoadConsaltantSpeciality(long iD, DataRow dr)
        {
            try
            {
                ClientConsultantSpecialilty specail = new ClientConsultantSpecialilty();
                specail.Where.ConsultantID.Value = iD;
                specail.Where.Active.Value = true;
                if (specail.Query.Load())
                {
                    int count = 1;
                    do
                    {
                        if (count == 1)
                        {
                            //HDN_ConsultantSpecialtyID.Value = specail.s_ID;
                            //DDL_ConsultantSpecialty.SelectedValue = specail.s_SpecialityID;
                            dr["ConsltSpecial1_ID"] = specail.s_ID;
                            dr["Special1"] = specail.s_SpecialityID;
                        }
                        if (count == 2)
                        {
                            //btnAddConsultantSpeciality.Visible = false;
                            //consultantSpeciality2.Visible = true;
                            //HDN_ConsultantSpecialtyID2.Value = specail.s_ID;
                            //DDL_ConsultantSpecialty2.SelectedValue = specail.s_SpecialityID;
                            dr["ConsltSpecial2_ID"] = specail.s_ID;
                            dr["Special2"] = specail.s_SpecialityID;
                        }
                        if (count == 3)
                        {
                            //btnAddConsultantSpeciality2.Visible = false;
                            //consultantSpeciality3.Visible = true;
                            //HDN_ConsultantSpecialityID3.Value = specail.s_ID;
                            //DDL_ConsultantSpeciality3.SelectedValue = specail.s_SpecialityID;
                            dr["ConsltSpecial3_ID"] = specail.s_ID;
                            dr["Special3"] = specail.s_SpecialityID;
                        }

                        count++;
                    } while (specail.MoveNext());
                }
            }
            catch (Exception ex)
            {
                LBL_MSG.Visible = true;
                LBL_MSG.Text = "LoadConsaltantSpeciality:" + ex.Message;
            }
        }

        #region Binding DropDownLists

        protected void BindAllDDLs()
        {
            BindCountryDDL();
            BindGovDDL();
            BindGovDDL2();
            BindGovDDL3();
            //BindConsGovDDL();
            BindSalesMen();
            BindFollowUpPeriod();
            BindSpeciality();
            LoadArea();
            LoadArea2();
            LoadArea3();
        }

        private void BindConsGovDDL(DropDownList ddlConsultantCountry, DropDownList ddlConsultantGovernorate)
        {
            GarasERP.Governorate governorate = new GarasERP.Governorate();
            //governorate.LoadAll();
            governorate.Where.CountryID.Value = ddlConsultantCountry.SelectedValue;
            governorate.Where.Active.Value = true;
            governorate.Query.AddOrderBy(Governorate.ColumnNames.Name, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
            ddlConsultantGovernorate.Items.Clear();
            if (governorate.Query.Load())
            {

                ddlConsultantGovernorate.DataSource = governorate.DefaultView;
                ddlConsultantGovernorate.DataTextField = GarasERP.Governorate.ColumnNames.Name;
                ddlConsultantGovernorate.DataValueField = GarasERP.Governorate.ColumnNames.ID;
                ddlConsultantGovernorate.DataBind();
            }
        }

        protected void BindSpeciality()
        {
            GarasERP.Speciality sp = new GarasERP.Speciality();
            sp.Where.Active.Value = true;
            sp.Query.AddOrderBy(Speciality.ColumnNames.Name, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
            if (sp.Query.Load())
            {

                ddlSpeciality.DataSource = sp.DefaultView;
                ddlSpeciality.DataTextField = GarasERP.Speciality.ColumnNames.Name;
                ddlSpeciality.DataValueField = GarasERP.Speciality.ColumnNames.ID;
                ddlSpeciality.DataBind();

                ddlSpeciality2.DataSource = sp.DefaultView;
                ddlSpeciality2.DataTextField = GarasERP.Speciality.ColumnNames.Name;
                ddlSpeciality2.DataValueField = GarasERP.Speciality.ColumnNames.ID;
                ddlSpeciality2.DataBind();

                ddlSpeciality3.DataSource = sp.DefaultView;
                ddlSpeciality3.DataTextField = GarasERP.Speciality.ColumnNames.Name;
                ddlSpeciality3.DataValueField = GarasERP.Speciality.ColumnNames.ID;
                ddlSpeciality3.DataBind();

                ddlSpeciality4.DataSource = sp.DefaultView;
                ddlSpeciality4.DataTextField = GarasERP.Speciality.ColumnNames.Name;
                ddlSpeciality4.DataValueField = GarasERP.Speciality.ColumnNames.ID;
                ddlSpeciality4.DataBind();

                VS_Speciality = sp.DefaultView.ToTable();

                //DDL_ConsultantSpecialty.DataSource = sp.DefaultView;
                //DDL_ConsultantSpecialty.DataTextField = GarasERP.Speciality.ColumnNames.Name;
                //DDL_ConsultantSpecialty.DataValueField = GarasERP.Speciality.ColumnNames.ID;
                //DDL_ConsultantSpecialty.DataBind();

                //DDL_ConsultantSpecialty2.DataSource = sp.DefaultView;
                //DDL_ConsultantSpecialty2.DataTextField = GarasERP.Speciality.ColumnNames.Name;
                //DDL_ConsultantSpecialty2.DataValueField = GarasERP.Speciality.ColumnNames.ID;
                //DDL_ConsultantSpecialty2.DataBind();

                //DDL_ConsultantSpeciality3.DataSource = sp.DefaultView;
                //DDL_ConsultantSpeciality3.DataTextField = GarasERP.Speciality.ColumnNames.Name;
                //DDL_ConsultantSpeciality3.DataValueField = GarasERP.Speciality.ColumnNames.ID;
                //DDL_ConsultantSpeciality3.DataBind();
            }
        }
        protected void BindCountryDDL()
        {
            Country country = new GarasERP.Country();
            country.Where.Active.Value = true;
            country.Query.AddOrderBy(Country.ColumnNames.Name, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
            if (country.Query.Load())
            {

                ddlCountry.DataSource = country.DefaultView;
                ddlCountry.DataTextField = GarasERP.Country.ColumnNames.Name;
                ddlCountry.DataValueField = GarasERP.Country.ColumnNames.ID;
                ddlCountry.DataBind();



                ddlCountry2.DataSource = country.DefaultView;
                ddlCountry2.DataTextField = GarasERP.Country.ColumnNames.Name;
                ddlCountry2.DataValueField = GarasERP.Country.ColumnNames.ID;
                ddlCountry2.DataBind();



                ddlCountry3.DataSource = country.DefaultView;
                ddlCountry3.DataTextField = GarasERP.Country.ColumnNames.Name;
                ddlCountry3.DataValueField = GarasERP.Country.ColumnNames.ID;
                ddlCountry3.DataBind();

                VS_Country = country.DefaultView.ToTable();

                //ddlConsultantCountry.DataSource = country.DefaultView;
                //ddlConsultantCountry.DataTextField = GarasERP.Country.ColumnNames.Name;
                //ddlConsultantCountry.DataValueField = GarasERP.Country.ColumnNames.ID;
                //ddlConsultantCountry.DataBind();
            }
        }

        protected void BindGovDDL()
        {
            ddlGovernate.Items.Clear();
            GarasERP.Governorate governorate = new GarasERP.Governorate();
            //governorate.LoadAll();
            governorate.Where.CountryID.Value = ddlCountry.SelectedValue;
            governorate.Where.Active.Value = true;
            governorate.Query.AddOrderBy(Governorate.ColumnNames.Name, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
            ddlGovernate.Items.Clear();
            if (governorate.Query.Load())
            {

                ddlGovernate.DataSource = governorate.DefaultView;
                ddlGovernate.DataTextField = GarasERP.Governorate.ColumnNames.Name;
                ddlGovernate.DataValueField = GarasERP.Governorate.ColumnNames.ID;
                ddlGovernate.DataBind();
            }


            //Governorate consultantGovernorate = new GarasERP.Governorate();
            //consultantGovernorate.Where.CountryID.Value = ddlConsultantCountry.SelectedValue;
            //consultantGovernorate.Query.Load();

            //ddlConsultantGovernorate.DataSource = consultantGovernorate.DefaultView;
            //ddlConsultantGovernorate.DataTextField = GarasERP.Governorate.ColumnNames.Name;
            //ddlConsultantGovernorate.DataValueField = GarasERP.Governorate.ColumnNames.ID;
            //ddlConsultantGovernorate.DataBind();
        }

        protected void BindGovDDL2()
        {
            ddlGovernate2.Items.Clear();
            GarasERP.Governorate governorate = new GarasERP.Governorate();
            //governorate.LoadAll();
            governorate.Where.CountryID.Value = ddlCountry2.SelectedValue;
            governorate.Where.Active.Value = true;
            governorate.Query.AddOrderBy(Governorate.ColumnNames.Name, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
            ddlGovernate2.Items.Clear();
            if (governorate.Query.Load())
            {

                ddlGovernate2.DataSource = governorate.DefaultView;
                ddlGovernate2.DataTextField = GarasERP.Governorate.ColumnNames.Name;
                ddlGovernate2.DataValueField = GarasERP.Governorate.ColumnNames.ID;
                ddlGovernate2.DataBind();
            }

        }

        protected void BindGovDDL3()
        {
            ddlGovernate3.Items.Clear();
            GarasERP.Governorate governorate = new GarasERP.Governorate();
            //governorate.LoadAll();
            governorate.Where.CountryID.Value = ddlCountry3.SelectedValue;
            governorate.Where.Active.Value = true;
            governorate.Query.AddOrderBy(Governorate.ColumnNames.Name, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
            ddlGovernate3.Items.Clear();
            if (governorate.Query.Load())
            {

                ddlGovernate3.DataSource = governorate.DefaultView;
                ddlGovernate3.DataTextField = GarasERP.Governorate.ColumnNames.Name;
                ddlGovernate3.DataValueField = GarasERP.Governorate.ColumnNames.ID;
                ddlGovernate3.DataBind();


                //GarasERP.Governorate consultantGovernorate = new GarasERP.Governorate();
                //consultantGovernorate.Where.CountryID.Value = ddlConsultantCountry.SelectedValue;
                //consultantGovernorate.Query.Load();

                //ddlConsultantGovernorate.DataSource = consultantGovernorate.DefaultView;
                //ddlConsultantGovernorate.DataTextField = GarasERP.Governorate.ColumnNames.Name;
                //ddlConsultantGovernorate.DataValueField = GarasERP.Governorate.ColumnNames.ID;
                //ddlConsultantGovernorate.DataBind();
            }
        }

        protected void BindSalesMen()
        {
            V_GroupUser_Branch users = new V_GroupUser_Branch();
            users.Where.BranchID.Value = Common.GetUserBranchID(UserID);
            users.Where.GroupName.Value = "SalesMen";
            users.Where.UserGroupActive.Value = true;
            users.Where.FirstName.Value = "System";
            users.Where.FirstName.Operator = MyGeneration.dOOdads.WhereParameter.Operand.NotEqual;
            users.Query.AddOrderBy(V_GroupUser_Branch.ColumnNames.FirstName, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
            users.Query.AddOrderBy(V_GroupUser_Branch.ColumnNames.LastName, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
            users.Query.AddResultColumn(V_GroupUser_Branch.ColumnNames.FirstName);
            users.Query.AddResultColumn(V_GroupUser_Branch.ColumnNames.LastName);
            users.Query.AddResultColumn(V_GroupUser_Branch.ColumnNames.UserID);
            ddlAssignedTo.Items.Clear();

            if (users.Query.Load())
            {
                if (users.DefaultView != null && users.DefaultView.Count > 0)
                {
                    do
                    {
                        ddlAssignedTo.Items.Add(new ListItem(users.FirstName + " " + users.LastName, users.s_UserID));

                    } while (users.MoveNext());
                }
            }

        }

        protected void BindFollowUpPeriod()
        {
            ddlFollowUpPeriod.DataSource = Enumerable.Range(1, 12);
            ddlFollowUpPeriod.DataBind();
        }

        protected void LoadArea()
        {
            try
            {
                DDL_Area.Items.Clear();
                if (ddlGovernate.SelectedValue != "")
                {
                    Area area = new Area();
                    area.Where.GovernorateID.Value = ddlGovernate.SelectedValue;
                    area.Query.AddOrderBy(Area.ColumnNames.Name, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
                    if (area.Query.Load())
                    {
                        if (area.DefaultView != null && area.DefaultView.Count > 0)
                        {
                            DDL_Area.DataSource = area.DefaultView;
                            DDL_Area.DataValueField = Area.ColumnNames.ID;
                            DDL_Area.DataTextField = Area.ColumnNames.Name;
                            DDL_Area.DataBind();

                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        protected void LoadArea2()
        {
            try
            {
                DDL_Area2.Items.Clear();
                if (ddlGovernate2.SelectedValue != "")
                {
                    Area area = new Area();
                    area.Where.GovernorateID.Value = ddlGovernate2.SelectedValue;
                    area.Query.AddOrderBy(Area.ColumnNames.Name, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
                    if (area.Query.Load())
                    {
                        if (area.DefaultView != null && area.DefaultView.Count > 0)
                        {
                            DDL_Area2.DataSource = area.DefaultView;
                            DDL_Area2.DataValueField = Area.ColumnNames.ID;
                            DDL_Area2.DataTextField = Area.ColumnNames.Name;
                            DDL_Area2.DataBind();

                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        protected void LoadArea3()
        {
            try
            {
                DDL_Area3.Items.Clear();
                if (ddlGovernate3.SelectedValue != "")
                {
                    Area area = new Area();
                    area.Where.GovernorateID.Value = ddlGovernate3.SelectedValue;
                    area.Query.AddOrderBy(Area.ColumnNames.Name, MyGeneration.dOOdads.WhereParameter.Dir.ASC);
                    if (area.Query.Load())
                    {
                        if (area.DefaultView != null && area.DefaultView.Count > 0)
                        {
                            DDL_Area3.DataSource = area.DefaultView;
                            DDL_Area3.DataValueField = Area.ColumnNames.ID;
                            DDL_Area3.DataTextField = Area.ColumnNames.Name;
                            DDL_Area3.DataBind();

                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        #endregion




        #region Adding Client Data
        protected bool ValidateDuplicates(string Flag)
        {
            if (Flag == "Name")
            {
                string ID = Encrypt_Decrypt.Decrypt(Page.Request.QueryString["CID"].ToString(), key);
                GarasERP.Client searchForClientinDB = new GarasERP.Client();
                searchForClientinDB.Where.Email.Value = tbxEmail.Text;
                //searchForClientinDB.Where.Email.Operator = MyGeneration.dOOdads.WhereParameter.Operand.Like;
                searchForClientinDB.Where.Email.Conjuction = MyGeneration.dOOdads.WhereParameter.Conj.Or;
                searchForClientinDB.Where.Name.Conjuction = MyGeneration.dOOdads.WhereParameter.Conj.Or;
                searchForClientinDB.Where.Name.Value = tbxCompanyName.Text;
               // searchForClientinDB.Where.Name.Operator = MyGeneration.dOOdads.WhereParameter.Operand.Like;

                searchForClientinDB.Where.WebSite.Conjuction = MyGeneration.dOOdads.WhereParameter.Conj.Or;
                searchForClientinDB.Where.WebSite.Value = tbxWebsite.Text;
               // searchForClientinDB.Where.WebSite.Operator = MyGeneration.dOOdads.WhereParameter.Operand.Like;
               // searchForClientinDB.Where.ID.Value = ID;
               // searchForClientinDB.Where.ID.Operator = MyGeneration.dOOdads.WhereParameter.Operand.NotEqual;
                searchForClientinDB.Query.Load();

                if (searchForClientinDB.RowCount > 0)
                {
                    bool result = true ;
                    do
                    {
                        if(searchForClientinDB.s_ID != ID)
                        {
                            result = false;
                            break;
                        }
                    } while (searchForClientinDB.MoveNext());
                    return result;
                }
                else
                    return true;
            }
            else if (Flag == "Mobile")
            {
                string ID = Encrypt_Decrypt.Decrypt(Page.Request.QueryString["CID"].ToString(), key);
                GarasERP.ClientMobile searchForClientMobile = new GarasERP.ClientMobile();
                searchForClientMobile.Where.Mobile.Value = tbxMobile.Text;
               
                searchForClientMobile.Query.Load();

                if (searchForClientMobile.RowCount > 0)
                {
                    bool result = true;
                    do
                    {
                        if (searchForClientMobile.s_ClientID != ID)
                        {
                            result = false;
                            break;
                        }
                    } while (searchForClientMobile.MoveNext());
                    return result;
                }
                else
                    return true;
            }
            else
                return true;

            //return true;
        }

        private void rebuildVS_Consultant(bool inSave)
        {
            initializeConsultDT();
            DataTable dt = VS_Consultant;
            foreach (RepeaterItem item in rptrConsaltant.Items)
            {
                string Consult_ID = ((HiddenField)item.FindControl("HDN_ConsultantID")).Value;
                string ConsltAddress_ID = ((HiddenField)item.FindControl("HDN_ddlConsultantAddressID")).Value;
                string ConsltEmail_ID = ((HiddenField)item.FindControl("HDN_ConsultantEmailID")).Value;
                string ConsltEmail1_ID = ((HiddenField)item.FindControl("HDN_ConsultantEmailID2")).Value;
                string ConsltEmail2_ID = ((HiddenField)item.FindControl("HDN_ConsultantEmailID3")).Value;
                string ConsltPhone_ID = ((HiddenField)item.FindControl("HDN_ConsultantPhoneID")).Value;
                string ConsltPhone1_ID = ((HiddenField)item.FindControl("HDN_ConsultantPhoneID2")).Value;
                string ConsltPhone2_ID = ((HiddenField)item.FindControl("HDN_ConsultantPhoneID3")).Value;
                string ConsltMobile_ID = ((HiddenField)item.FindControl("HDN_ConsultantMobileID")).Value;
                string ConsltMobile1_ID = ((HiddenField)item.FindControl("HDN_ConsultantMobileID2")).Value;
                string ConsltMobile2_ID = ((HiddenField)item.FindControl("HDN_ConsultantMobileID3")).Value;
                string ConsltFax_ID = ((HiddenField)item.FindControl("HDN_ConsultantFaxID")).Value;
                string ConsltFax1_ID = ((HiddenField)item.FindControl("HDN_ConsultantFaxID2")).Value;
                string ConsltFax2_ID = ((HiddenField)item.FindControl("HDN_ConsultantFaxID3")).Value;
                string ConsltSpecial1_ID = ((HiddenField)item.FindControl("HDN_ConsultantSpecialtyID")).Value;
                string ConsltSpecial2_ID = ((HiddenField)item.FindControl("HDN_ConsultantSpecialtyID2")).Value;
                string ConsltSpecial3_ID = ((HiddenField)item.FindControl("HDN_ConsultantSpecialityID3")).Value;
                string ConsltName = ((TextBox)item.FindControl("tbxConsultantName")).Text;
                string ConsltOffice = ((TextBox)item.FindControl("tbxConsultantOffice")).Text;
                string ConsltFor = ((DropDownList)item.FindControl("DDl_For")).SelectedValue;
                string ConsltCountry = ((DropDownList)item.FindControl("ddlConsultantCountry")).SelectedValue;
                string ConsltGvrnrt = ((DropDownList)item.FindControl("ddlConsultantGovernorate")).SelectedValue;
                string ConsltStreet = ((TextBox)item.FindControl("tbxConsultantStreet")).Text;
                string ConsltBuilding = ((TextBox)item.FindControl("tbxConsultantBuilding")).Text;
                string ConsltFloor = ((TextBox)item.FindControl("tbxConsultantFloor")).Text;
                string ConsltDesc = ((TextBox)item.FindControl("tbxConsultantDescription")).Text;
                string ConsltEmail = ((TextBox)item.FindControl("tbxConsultantEmail")).Text;
                string ConsltEmail1 = ((TextBox)item.FindControl("tbxConsultantEmail2")).Text;
                string ConsltEmail2 = ((TextBox)item.FindControl("tbxConsultantEmail3")).Text;
                string ConsltPhone = ((TextBox)item.FindControl("tbxConsultantPhone")).Text;
                string ConsltPhone1 = ((TextBox)item.FindControl("tbxConsultantPhone2")).Text;
                string ConsltPhone2 = ((TextBox)item.FindControl("tbxConsultantPhone3")).Text;
                string ConsltMobile = ((TextBox)item.FindControl("tbxConsultantMobile")).Text;
                string ConsltMobile1 = ((TextBox)item.FindControl("tbxConsultantMobile2")).Text;
                string ConsltMobile2 = ((TextBox)item.FindControl("tbxConsultantMobile3")).Text;
                string ConsltFax = ((TextBox)item.FindControl("tbxConsultantFax")).Text;
                string ConsltFax1 = ((TextBox)item.FindControl("tbxConsultantFax2")).Text;
                string ConsltFax2 = ((TextBox)item.FindControl("tbxConsultantFax3")).Text;
                string Special1 = ((DropDownList)item.FindControl("DDL_ConsultantSpecialty")).SelectedValue;
                string Special2 = ((DropDownList)item.FindControl("DDL_ConsultantSpecialty2")).SelectedValue;
                string Special3 = ((DropDownList)item.FindControl("ddlSpeciality3")).SelectedValue;
                if (item.ItemType == ListItemType.Item
                    || item.ItemType == ListItemType.AlternatingItem
                    || (inSave && item.ItemType == ListItemType.Footer && ((TextBox)item.FindControl("tbxConsultantName")).Text.Trim() != ""))
                {
                    if (chbxAvailable.Checked)
                    {
                        ((RequiredFieldValidator)item.FindControl("reqFldVldConsltName")).Enabled = true;
                    }
                    else
                    {
                        ((RequiredFieldValidator)item.FindControl("reqFldVldConsltName")).Enabled = false;
                    }
                    DataRow dr = dt.NewRow();
                    dr["Consult_ID"] = Consult_ID;
                    dr["ConsltAddress_ID"] = ConsltAddress_ID;
                    dr["ConsltEmail_ID"] = ConsltEmail_ID;
                    dr["ConsltEmail1_ID"] = ConsltEmail1_ID;
                    dr["ConsltEmail2_ID"] = ConsltEmail2_ID;
                    dr["ConsltPhone_ID"] = ConsltPhone_ID;
                    dr["ConsltPhone1_ID"] = ConsltPhone1_ID;
                    dr["ConsltPhone2_ID"] = ConsltPhone2_ID;
                    dr["ConsltMobile_ID"] = ConsltMobile_ID;
                    dr["ConsltMobile1_ID"] = ConsltMobile1_ID;
                    dr["ConsltMobile2_ID"] = ConsltMobile2_ID;
                    dr["ConsltFax_ID"] = ConsltFax_ID;
                    dr["ConsltFax1_ID"] = ConsltFax1_ID;
                    dr["ConsltFax2_ID"] = ConsltFax2_ID;
                    dr["ConsltSpecial1_ID"] = ConsltSpecial1_ID;
                    dr["ConsltSpecial2_ID"] = ConsltSpecial2_ID;
                    dr["ConsltSpecial3_ID"] = ConsltSpecial3_ID; 
                    dr["ConsltName"] = ConsltName;
                    dr["ConsltOffice"] = ConsltOffice;
                    dr["ConsltFor"] = ConsltFor;
                    dr["ConsltCountry"] = ConsltCountry;
                    dr["ConsltGvrnrt"] = ConsltGvrnrt;
                    dr["ConsltStreet"] = ConsltStreet;
                    dr["ConsltBuilding"] = ConsltBuilding;
                    dr["ConsltFloor"] = ConsltFloor;
                    dr["ConsltDesc"] = ConsltDesc;
                    dr["ConsltEmail"] = ConsltEmail;
                    dr["ConsltEmail1"] = ConsltEmail1;
                    dr["ConsltEmail2"] = ConsltEmail2;
                    dr["ConsltPhone"] = ConsltPhone;
                    dr["ConsltPhone1"] = ConsltPhone1;
                    dr["ConsltPhone2"] = ConsltPhone2;
                    dr["ConsltMobile"] = ConsltMobile;
                    dr["ConsltMobile1"] = ConsltMobile1;
                    dr["ConsltMobile2"] = ConsltMobile2;
                    dr["ConsltFax"] = ConsltFax;
                    dr["ConsltFax1"] = ConsltFax1;
                    dr["ConsltFax2"] = ConsltFax2;
                    dr["Special1"] = Special1;
                    dr["Special2"] = Special2;
                    dr["Special3"] = Special3;
                    dt.Rows.Add(dr);
                }
                if (item.ItemType == ListItemType.Footer)
                {
                    DataRow dr = dt.NewRow();
                    dr["ConsltName"] = ConsltName;
                    dr["ConsltOffice"] = ConsltOffice;
                    dr["ConsltFor"] = ConsltFor;
                    dr["ConsltCountry"] = ConsltCountry;
                    dr["ConsltGvrnrt"] = ConsltGvrnrt;
                    dr["ConsltStreet"] = ConsltStreet;
                    dr["ConsltBuilding"] = ConsltBuilding;
                    dr["ConsltFloor"] = ConsltFloor;
                    dr["ConsltDesc"] = ConsltDesc;
                    dr["ConsltEmail"] = ConsltEmail;
                    dr["ConsltEmail1"] = ConsltEmail1;
                    dr["ConsltEmail2"] = ConsltEmail2;
                    dr["ConsltPhone"] = ConsltPhone;
                    dr["ConsltPhone1"] = ConsltPhone1;
                    dr["ConsltPhone2"] = ConsltPhone2;
                    dr["ConsltMobile"] = ConsltMobile;
                    dr["ConsltMobile1"] = ConsltMobile1;
                    dr["ConsltMobile2"] = ConsltMobile2;
                    dr["ConsltFax"] = ConsltFax;
                    dr["ConsltFax1"] = ConsltFax1;
                    dr["ConsltFax2"] = ConsltFax2;
                    dr["Special1"] = Special1;
                    dr["Special2"] = Special2;
                    dr["Special3"] = Special3;
                    VS_Consultant_Footer = dr;
                }
            }
            VS_Consultant = dt;
            rptrConsaltant.DataSource = VS_Consultant;
            rptrConsaltant.DataBind();
        }

        protected void InsertClientData()
        {
            string script = "";
            try
            {
                GarasERP.Client client = new GarasERP.Client();
                if (FillClientName(client))
                {
                    FillClientAddress(client);
                    FillClientContacts(client);
                    FillClientContactPerson(client);
                    Fill_Client_Specialty(client);
                    if (chbxAvailable.Checked)
                    {
                        rebuildVS_Consultant(true);
                        foreach (DataRow consult in VS_Consultant.Rows)
                        {
                            if (consult["Consult_ID"].ToString() != "")
                            {
                                long consID = 0;
                                if (long.TryParse(consult["Consult_ID"].ToString(), out consID))
                                {
                                    ClientConsultantAddress address = new ClientConsultantAddress();
                                    address.Where.ConsultantID.Value = consID;
                                    if (address.Query.Load())
                                    {
                                        address.DeleteAll();
                                        address.Save();
                                    }

                                    ClientConsultantEmail email = new ClientConsultantEmail();
                                    email.Where.ConsultantID.Value = consID;
                                    if (email.Query.Load())
                                    {
                                        email.DeleteAll();
                                        email.Save();
                                    }

                                    ClientConsultantFax fax = new ClientConsultantFax();
                                    fax.Where.ConsultantID.Value = consID;
                                    if (fax.Query.Load())
                                    {
                                        fax.DeleteAll();
                                        fax.Save();
                                    }

                                    ClientConsultantMobile mobile = new ClientConsultantMobile();
                                    mobile.Where.ConsultantID.Value = consID;
                                    if (mobile.Query.Load())
                                    {
                                        mobile.DeleteAll();
                                        mobile.Save();
                                    }

                                    ClientConsultantPhone phone = new ClientConsultantPhone();
                                    phone.Where.ConsultantID.Value = consID;
                                    if (phone.Query.Load())
                                    {
                                        phone.DeleteAll();
                                        phone.Save();
                                    }

                                    ClientConsultantSpecialilty spic = new ClientConsultantSpecialilty();
                                    spic.Where.ConsultantID.Value = consID;
                                    if (spic.Query.Load())
                                    {
                                        spic.DeleteAll();
                                        spic.Save();
                                    }

                                    ClientConsultant cons = new ClientConsultant();

                                    if (cons.LoadByPrimaryKey(consID))
                                    {
                                        cons.MarkAsDeleted();
                                        cons.Save();
                                    }

                                }
                            }
                            else
                            {
                                GarasERP.ClientConsultant consultant = new GarasERP.ClientConsultant();
                                selectedConsultantName = consult["ConsltName"].ToString();
                                InsertClientConsultantData(client, consultant, consult);
                            }
                            //client.Consultant = selectedConsultantName; 
                        }
                        client.Save();
                    }
                    
                    //if (rbConsultantIndiv.Checked == true)
                    //{
                    //    GarasERP.ClientConsultant consultant = new GarasERP.ClientConsultant();
                        
                    //    selectedConsultantName = tbxConsultantName.Text;
                    //    InsertClientConsultantData(client, consultant);
                    //    client.Consultant = selectedConsultantName;
                    //    client.Save();
                    //}
                    //else if (rbConsultantCompany.Checked == true)
                    //{
                    //    GarasERP.ClientConsultant consultant = new GarasERP.ClientConsultant();
                    //    selectedConsultantName = tbxConsultantOffice.Text;
                    //    InsertClientConsultantData(client, consultant);
                    //    client.Consultant = selectedConsultantName;
                    //    client.Save();
                    //}
                    //else
                    //{
                    //    if(HDN_ConsultantID.Value!="")
                    //    {
                    //        long consID = 0;
                    //        if (long.TryParse(HDN_ConsultantID.Value, out consID))
                    //        {
                    //            ClientConsultantAddress address = new ClientConsultantAddress();
                    //            address.Where.ConsultantID.Value = consID;
                    //            if(address.Query.Load())
                    //            {
                    //                address.DeleteAll();
                    //                address.Save();
                    //            }

                    //            ClientConsultantEmail email = new ClientConsultantEmail();
                    //            email.Where.ConsultantID.Value = consID;
                    //            if (email.Query.Load())
                    //            {
                    //                email.DeleteAll();
                    //                email.Save();
                    //            }

                    //            ClientConsultantFax fax = new ClientConsultantFax();
                    //            fax.Where.ConsultantID.Value = consID;
                    //            if (fax.Query.Load())
                    //            {
                    //                fax.DeleteAll();
                    //                fax.Save();
                    //            }

                    //            ClientConsultantMobile mobile = new ClientConsultantMobile();
                    //            mobile.Where.ConsultantID.Value = consID;
                    //            if (mobile.Query.Load())
                    //            {
                    //                mobile.DeleteAll();
                    //                mobile.Save();
                    //            }

                    //            ClientConsultantPhone phone = new ClientConsultantPhone();
                    //            phone.Where.ConsultantID.Value = consID;
                    //            if (phone.Query.Load())
                    //            {
                    //                phone.DeleteAll();
                    //                phone.Save();
                    //            }

                    //            ClientConsultantSpecialilty spic = new ClientConsultantSpecialilty();
                    //            spic.Where.ConsultantID.Value = consID;
                    //            if (spic.Query.Load())
                    //            {
                    //                spic.DeleteAll();
                    //                spic.Save();
                    //            }

                    //            ClientConsultant cons = new ClientConsultant();
                                
                    //            if (cons.LoadByPrimaryKey(consID))
                    //            {
                    //                cons.MarkAsDeleted();
                    //                cons.Save();
                    //            }

                    //        }
                    //    }
                    //}

                    //if (uploadPhoto.HasFile)
                    //{
                    //    client.Logo = uploadPhoto.FileBytes;
                    //    client.Save();
                    //}
                    CreateAttachmentDirectory(client);
                    CreateNotification(client);
                    script = "alert('Client Registered Successfully!');";
                    this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "Registeration Successful", script, true /* addScriptTags */);
                    //this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "Registeration Successful", "javascript:window.open( '/Sales/Client/MyClients.aspx');", true /* addScriptTags */);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "Registeration Successful", "javascript:window.location.href('/Sales/Client/MyClients.aspx');", true /* addScriptTags */);

                }
                else
                {
                    script = "alert('A Client with the same data already exists!');";
                    Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "Duplicate Record", script, true /* addScriptTags */);
                }
                //Response.Redirect("/Sales/SalesHome.aspx", false);
            }
            catch (Exception ex)
            {
                //Log exception
                script = "alert('Client Registration cannot be done right now, Please try again later!');";
                Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "Registeration Failed", script, true /* addScriptTags */);
            }
        }

        protected bool FillClientName(GarasERP.Client client)
        {
            if (ValidateDuplicates("Name") == false || !ValidateDuplicates("Mobile"))
            {
                return false;
            }
            else
            {
                if (tbxCompanyName.Text.Trim() != "" && ddlAssignedTo.Items != null && ddlAssignedTo.SelectedValue != "")
                {
                    string ID = Encrypt_Decrypt.Decrypt(Page.Request.QueryString["CID"].ToString(), key);
                    if (client.LoadByPrimaryKey(long.Parse(ID)))
                    {
                        client.Name = tbxCompanyName.Text;
                        client.GroupName = tbxGroupName.Text;
                        client.BranchName = tbxBranch.Text;
                        client.Type = selectedRadio;

                        client.Email = tbxEmail.Text;
                        client.WebSite = tbxWebsite.Text;
                       // client. = UserID;
                        //client.CreationDate = DateTime.Now;
                        ViewState["OldSalesID"] = client.SalesPersonID;
                        client.SalesPersonID = long.Parse(ddlAssignedTo.SelectedValue);
                        client.Note = tbxNote.Text;
                        //client.Rate = 1;

                        client.FollowUpPeriod = int.Parse(ddlFollowUpPeriod.SelectedValue);

                        //if (rbConsultantNA.Checked)
                        //    client.ConsultantType = "N/A";
                        //if (rbConsultantCompany.Checked)
                        //    client.ConsultantType = "Company / Office";
                        //if (rbConsultantIndiv.Checked)
                        //    client.ConsultantType = "Individual";
                        if (!chbxAvailable.Checked)
                            client.ConsultantType = "N/A";
                        else
                        {
                            client.ConsultantType = "Has Consultant";
                        }

                        if (uploadPhoto.HasFile)
                        {
                            client.Logo = uploadPhoto.FileBytes;
                            client.HasLogo = true;
                            //client.Save();
                        }

                        client.SupportedByCompany = CHB_SupportedByCompany.Checked;
                        if (CHB_SupportedByCompany.Checked)
                        {
                            client.SupportedBy = DDL_Supported.SelectedValue;
                        }

                        // client.Rate = 0;

                        client.Save();
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }
        protected void FillClientAddress(GarasERP.Client client)
        {
            if (ddlCountry.Items != null && ddlGovernate.Items != null && ddlCountry.SelectedValue != "" && ddlGovernate.SelectedValue != "" )
            {
                GarasERP.ClientAddress clientAddress = new GarasERP.ClientAddress();
                if (HDN_AddressID.Value == "")
                {
                    clientAddress.AddNew();
                    clientAddress.CreatedBy = UserID;
                    clientAddress.CreationDate = DateTime.Now;
                }
                else
                {
                    clientAddress.LoadByPrimaryKey(long.Parse(HDN_AddressID.Value));
                }
                clientAddress.ClientID = client.ID;
                clientAddress.CountryID = int.Parse(ddlCountry.SelectedValue);
                clientAddress.GovernorateID = int.Parse(ddlGovernate.SelectedValue);
                clientAddress.Address = tbxStreet.Text;
                clientAddress.Modified = DateTime.Now;
                clientAddress.ModifiedBy = UserID;
                clientAddress.Active = true;
                clientAddress.BuildingNumber = tbxBuilding.Text;
                clientAddress.Floor = tbxFloor.Text;
                clientAddress.Description = tbxDesc.Text;
                if (DDL_Area.Items != null && DDL_Area.Items.Count > 0 && DDL_Area.SelectedValue != "")
                    clientAddress.AreaID = long.Parse(DDL_Area.SelectedValue);
                clientAddress.Save();
            }

            if (address2.Visible && ddlCountry2.Items != null && ddlGovernate2.Items != null && ddlCountry2.SelectedValue != "" && ddlGovernate2.SelectedValue != "" )
            {
                GarasERP.ClientAddress clientAddress2 = new GarasERP.ClientAddress();
                if (HDN_AddressID2.Value == "")
                {
                    clientAddress2.AddNew();
                    clientAddress2.CreatedBy = UserID;
                    clientAddress2.CreationDate = DateTime.Now;
                }
                else
                {
                    clientAddress2.LoadByPrimaryKey(long.Parse(HDN_AddressID2.Value));
                }
               
                clientAddress2.ClientID = client.ID;
                clientAddress2.CountryID = int.Parse(ddlCountry2.SelectedValue);
                clientAddress2.GovernorateID = int.Parse(ddlGovernate2.SelectedValue);
                clientAddress2.Address = tbxStreet2.Text;
                clientAddress2.ModifiedBy = UserID;
                clientAddress2.Modified = DateTime.Now;
                clientAddress2.Active = true;
                clientAddress2.BuildingNumber = tbxBuilding2.Text;
                clientAddress2.Floor = tbxFloor2.Text;
                clientAddress2.Description = tbxDesc2.Text;
                if (DDL_Area2.Items != null && DDL_Area2.Items.Count > 0 && DDL_Area2.SelectedValue != "")
                    clientAddress2.AreaID = long.Parse(DDL_Area2.SelectedValue);
                clientAddress2.Save();
            }
            if (!address2.Visible && HDN_AddressID2.Value!="")
            {
                GarasERP.ClientAddress clientAddress2 = new GarasERP.ClientAddress();

                if(clientAddress2.LoadByPrimaryKey(long.Parse(HDN_AddressID2.Value)))
                {
                    clientAddress2.MarkAsDeleted();
                    clientAddress2.Save();
                }
               
            }

            if (address3.Visible && ddlCountry3.Items != null && ddlGovernate3.Items != null && ddlCountry3.SelectedValue != "" && ddlGovernate3.SelectedValue != "" && tbxStreet3.Text.Trim() != "")
            {
                GarasERP.ClientAddress clientAddress3 = new GarasERP.ClientAddress();
                if (HDN_AddressID3.Value == "")
                {
                    clientAddress3.AddNew();
                    clientAddress3.CreatedBy = UserID;
                    clientAddress3.CreationDate = DateTime.Now;
                }
                else
                {
                    clientAddress3.LoadByPrimaryKey(long.Parse(HDN_AddressID3.Value));
                }
                clientAddress3.ClientID = client.ID;
                clientAddress3.CountryID = int.Parse(ddlCountry3.SelectedValue);
                clientAddress3.GovernorateID = int.Parse(ddlGovernate3.SelectedValue);
                clientAddress3.Address = tbxStreet3.Text;
                clientAddress3.ModifiedBy = UserID;
                clientAddress3.Modified = DateTime.Now;
                clientAddress3.Active = true;
                clientAddress3.BuildingNumber = tbxBuilding3.Text;
                clientAddress3.Floor = tbxFloor3.Text;
                clientAddress3.Description = tbxDesc3.Text;
                if (DDL_Area3.Items != null && DDL_Area3.Items.Count > 0 && DDL_Area3.SelectedValue != "")
                    clientAddress3.AreaID = long.Parse(DDL_Area3.SelectedValue);
                clientAddress3.Save();
            }
            if (!address3.Visible && HDN_AddressID3.Value != "")
            {
                GarasERP.ClientAddress clientAddress3 = new GarasERP.ClientAddress();

                if (clientAddress3.LoadByPrimaryKey(long.Parse(HDN_AddressID3.Value)))
                {
                    clientAddress3.MarkAsDeleted();
                    clientAddress3.Save();
                }

            }
        }
        protected bool FillClientContacts(GarasERP.Client client)
        {
            if (ValidateDuplicates("Mobile") == false)
            {
                return false;
            }
            else
            {
                if (tbxMobile.Text.Trim() != "")
                {
                    GarasERP.ClientMobile clientMobile = new GarasERP.ClientMobile();
                    if (HDN_MobileID.Value == "")
                    {
                        clientMobile.AddNew();
                        clientMobile.CreatedBy = UserID;
                        clientMobile.CreationDate = DateTime.Now;
                    }
                    else
                    {
                        clientMobile.LoadByPrimaryKey(long.Parse(HDN_MobileID.Value));
                    }
                    clientMobile.ClientID = client.ID;
                    clientMobile.Mobile = tbxMobile.Text == String.Empty ? "N/A" : tbxMobile.Text;
                    clientMobile.Active = true;
                    clientMobile.CreatedBy = UserID;
                    clientMobile.CreationDate = DateTime.Now;
                    clientMobile.Save();
                }
                else
                {
                    if (HDN_MobileID.Value != "")
                    {
                        GarasERP.ClientMobile clientMobile = new GarasERP.ClientMobile();
                        if(clientMobile.LoadByPrimaryKey(long.Parse(HDN_MobileID.Value)))
                        {
                            clientMobile.MarkAsDeleted();
                            clientMobile.Save();
                        }
                    }
                }

                if (newMobile2.Visible && tbxMobile2.Text.Trim() != "")
                {
                    GarasERP.ClientMobile clientMobile2 = new GarasERP.ClientMobile();
                    if (HDN_MobileID2.Value == "")
                    {
                        clientMobile2.AddNew();
                        clientMobile2.CreatedBy = UserID;
                        clientMobile2.CreationDate = DateTime.Now;
                    }
                    else
                    {
                        clientMobile2.LoadByPrimaryKey(long.Parse(HDN_MobileID2.Value));
                    }
                    
                    clientMobile2.ClientID = client.ID;
                    clientMobile2.Mobile = tbxMobile2.Text == String.Empty ? "N/A" : tbxMobile2.Text; ;
                    clientMobile2.Active = true;
                    clientMobile2.ModifiedBy = UserID;
                    clientMobile2.Modified = DateTime.Now;
                    clientMobile2.Save();
                }
                if(!newMobile2.Visible && HDN_MobileID2.Value != "")
                {
                    GarasERP.ClientMobile clientMobile2 = new GarasERP.ClientMobile();
                    if(clientMobile2.LoadByPrimaryKey(long.Parse(HDN_MobileID2.Value)))
                    {
                        clientMobile2.MarkAsDeleted();
                        clientMobile2.Save();
                    }

                }

                if (newMobile3.Visible && tbxMobile3.Text.Trim() != "")
                {
                    GarasERP.ClientMobile clientMobile3 = new GarasERP.ClientMobile();
                    if (HDN_MobileID3.Value == "")
                    {
                        clientMobile3.AddNew();
                        clientMobile3.CreatedBy = UserID;
                        clientMobile3.CreationDate = DateTime.Now;
                    }
                    else
                    {
                        clientMobile3.LoadByPrimaryKey(long.Parse(HDN_MobileID3.Value));
                    }
                    
                    clientMobile3.ClientID = client.ID;
                    clientMobile3.Mobile = tbxMobile3.Text == String.Empty ? "N/A" : tbxMobile3.Text; ;
                    clientMobile3.Active = true;
                    clientMobile3.ModifiedBy = UserID;
                    clientMobile3.Modified = DateTime.Now;
                    clientMobile3.Save();
                }

                if (!newMobile3.Visible && HDN_MobileID3.Value != "")
                {
                    GarasERP.ClientMobile clientMobile3 = new GarasERP.ClientMobile();
                    if (clientMobile3.LoadByPrimaryKey(long.Parse(HDN_MobileID3.Value)))
                    {
                        clientMobile3.MarkAsDeleted();
                        clientMobile3.Save();
                    }

                }

                
                if (tbxPhone.Text.Trim() != "")
                {
                    GarasERP.ClientPhone clientPhone = new GarasERP.ClientPhone();
                    if (HDN_PhoneID.Value == "")
                    {
                        clientPhone.AddNew();
                        clientPhone.CreatedBy = UserID;
                        clientPhone.CreationDate = DateTime.Now;
                    }
                    else
                        clientPhone.LoadByPrimaryKey(long.Parse(HDN_PhoneID.Value));
                    clientPhone.ClientID = client.ID;
                    clientPhone.Phone = tbxPhone.Text == String.Empty ? "N/A" : tbxPhone.Text;
                    clientPhone.Active = true;
                    clientPhone.ModifiedBy = UserID;
                    clientPhone.Modified = DateTime.Now;
                    clientPhone.Save();
                }
                else
                {
                    if (HDN_PhoneID.Value != "")
                    {
                        GarasERP.ClientPhone clientPhone = new GarasERP.ClientPhone();
                        if(clientPhone.LoadByPrimaryKey(long.Parse(HDN_PhoneID.Value)))
                        {
                            clientPhone.MarkAsDeleted();
                            clientPhone.Save();
                        }
                    }
                }

                if (newPhone.Visible && tbxPhone2.Text.Trim() != "")
                {
                    GarasERP.ClientPhone clientPhone2 = new GarasERP.ClientPhone();
                    if (HDN_PhoneID2.Value == "")
                    {
                        clientPhone2.AddNew();
                        clientPhone2.CreatedBy = UserID;
                        clientPhone2.CreationDate = DateTime.Now;
                    }
                    else
                        clientPhone2.LoadByPrimaryKey(long.Parse(HDN_PhoneID2.Value));
                   
                    clientPhone2.ClientID = client.ID;
                    clientPhone2.Phone = tbxPhone2.Text == String.Empty ? "N/A" : tbxPhone2.Text;
                    clientPhone2.Active = true;
                    clientPhone2.ModifiedBy = UserID;
                    clientPhone2.Modified = DateTime.Now;
                    clientPhone2.Save();
                }
                if(!newPhone.Visible && HDN_PhoneID2.Value != "")
                {
                    GarasERP.ClientPhone clientPhone2 = new GarasERP.ClientPhone();
                    if(clientPhone2.LoadByPrimaryKey(long.Parse(HDN_PhoneID2.Value)))
                    {
                        clientPhone2.MarkAsDeleted();
                        clientPhone2.Save();
                    }

                }

                if (newPhone3.Visible && tbxPhone3.Text.Trim() != "")
                {
                    GarasERP.ClientPhone clientPhone3 = new GarasERP.ClientPhone();
                    if (HDN_PhoneID3.Value == "")
                    {
                        clientPhone3.AddNew();
                        clientPhone3.CreatedBy = UserID;
                        clientPhone3.CreationDate = DateTime.Now;
                    }
                    else
                        clientPhone3.LoadByPrimaryKey(long.Parse(HDN_PhoneID3.Value));
                    
                    clientPhone3.ClientID = client.ID;
                    clientPhone3.Phone = tbxPhone3.Text == String.Empty ? "N/A" : tbxPhone3.Text;
                    clientPhone3.Active = true;
                    clientPhone3.ModifiedBy = UserID;
                    clientPhone3.Modified = DateTime.Now;
                    clientPhone3.Save();
                }

                if (!newPhone3.Visible && HDN_PhoneID3.Value != "")
                {
                    GarasERP.ClientPhone clientPhone3 = new GarasERP.ClientPhone();
                    if (clientPhone3.LoadByPrimaryKey(long.Parse(HDN_PhoneID3.Value)))
                    {
                        clientPhone3.MarkAsDeleted();
                        clientPhone3.Save();
                    }

                }

                if (tbxFax.Text.Trim() != "")
                {
                    GarasERP.ClientFax clientFax = new GarasERP.ClientFax();
                    if (HDN_FAXID.Value == "")
                    {
                        clientFax.AddNew();
                        clientFax.CreatedBy = UserID;
                        clientFax.CreationDate = DateTime.Now;
                    }
                    else
                        clientFax.LoadByPrimaryKey(long.Parse(HDN_FAXID.Value));
                   
                    clientFax.ClientID = client.ID;
                    clientFax.Fax = tbxFax.Text == String.Empty ? "N/A" : tbxFax.Text;
                    clientFax.Active = true;
                    clientFax.ModifiedBy = UserID;
                    clientFax.Modified = DateTime.Now;
                    clientFax.Save();
                }
                else
                {
                    if (HDN_FAXID.Value != "")
                    {
                        GarasERP.ClientFax clientFax = new GarasERP.ClientFax();
                        if(clientFax.LoadByPrimaryKey(long.Parse(HDN_FAXID.Value)))
                        {
                            clientFax.MarkAsDeleted();
                            clientFax.Save();
                        }
                    }
                }

                if (newFax2.Visible && tbxFax2.Text.Trim() != "")
                {
                    GarasERP.ClientFax clientFax2 = new GarasERP.ClientFax();
                    if (HDN_FaxID2.Value == "")
                    {
                        clientFax2.AddNew();
                        clientFax2.CreatedBy = UserID;
                        clientFax2.CreationDate = DateTime.Now;
                    }
                    else
                        clientFax2.LoadByPrimaryKey(long.Parse(HDN_FaxID2.Value));
                    clientFax2.ClientID = client.ID;
                    clientFax2.Fax = tbxFax2.Text == String.Empty ? "N/A" : tbxFax2.Text;
                    clientFax2.Active = true;
                    clientFax2.ModifiedBy = UserID;
                    clientFax2.Modified = DateTime.Now;
                    clientFax2.Save();
                }
                if(!newFax2.Visible && HDN_FaxID2.Value != "")
                {
                    GarasERP.ClientFax clientFax2 = new GarasERP.ClientFax();
                    if(clientFax2.LoadByPrimaryKey(long.Parse(HDN_FaxID2.Value)))
                    {
                        clientFax2.MarkAsDeleted();
                        clientFax2.Save();
                    }
                }

                if (newFax3.Visible && tbxFax3.Text.Trim() != "")
                {
                    GarasERP.ClientFax clientFax3 = new GarasERP.ClientFax();
                    if (HDN_FaxeID3.Value == "")
                    {
                        clientFax3.AddNew();
                        clientFax3.CreatedBy = UserID;
                        clientFax3.CreationDate = DateTime.Now;
                    }
                    else
                        clientFax3.LoadByPrimaryKey(long.Parse(HDN_FaxeID3.Value));
                    clientFax3.ClientID = client.ID;
                    clientFax3.Fax = tbxFax3.Text == String.Empty ? "N/A" : tbxFax3.Text;
                    clientFax3.Active = true;
                    clientFax3.ModifiedBy = UserID;
                    clientFax3.Modified = DateTime.Now;
                    clientFax3.Save();
                }
                if (!newFax3.Visible && HDN_FaxeID3.Value != "")
                {
                    GarasERP.ClientFax clientFax3 = new GarasERP.ClientFax();
                    if(clientFax3.LoadByPrimaryKey(long.Parse(HDN_FaxeID3.Value)))
                    {
                        clientFax3.MarkAsDeleted();
                        clientFax3.Save();
                    }
                }

                    return true;
            }
        }
        protected void FillClientContactPerson(GarasERP.Client client)
        {
            
            if (tbxContactPerson.Text.Trim() != "" && tbxContactTitle.Text.Trim() != "" && tbxContactMobile.Text.Trim() != "")
            {
                GarasERP.ClientContactPerson contact = new GarasERP.ClientContactPerson();
                if (HDN_ContactID.Value == "")
                {
                    contact.AddNew();
                    contact.CreatedBy = UserID;
                    contact.CreationDate = DateTime.Now;
                }
                else
                    contact.LoadByPrimaryKey(long.Parse(HDN_ContactID.Value));
                contact.ClientID = client.ID;
                contact.ModifiedBy = UserID;
                contact.Modified = DateTime.Now;
                contact.Active = true;
                contact.Name = tbxContactPerson.Text;
                contact.Title = tbxContactTitle.Text;
                contact.Location = tbxContactLocation.Text;
                contact.Email = tbxContactEmail.Text;
                contact.Mobile = tbxContactMobile.Text;
                contact.Save();
            }
            else
            {
                if (HDN_ContactID.Value != "")
                {
                    GarasERP.ClientContactPerson contact = new GarasERP.ClientContactPerson();
                    if(contact.LoadByPrimaryKey(long.Parse(HDN_ContactID.Value)))
                    {
                        contact.MarkAsDeleted();
                        contact.Save();
                    }
                }
            }

            if (newContact2.Visible && tbxContactName2.Text.Trim() != "" && tbxContactTitle2.Text.Trim() != "" && tbxContactMobile2.Text.Trim() != "")
            {
                GarasERP.ClientContactPerson contact2 = new GarasERP.ClientContactPerson();

                if (HDN_ContactID1.Value == "")
                {
                    contact2.AddNew();
                    contact2.CreatedBy = UserID;
                    contact2.CreationDate = DateTime.Now;
                }
                else
                    contact2.LoadByPrimaryKey(long.Parse(HDN_ContactID1.Value));

                
                contact2.ClientID = client.ID;
                contact2.ModifiedBy = UserID;
                contact2.Modified = DateTime.Now;
                contact2.Active = true;
                contact2.Name = tbxContactName2.Text;
                contact2.Title = tbxContactTitle2.Text;
                contact2.Location = tbxContactLocation2.Text;
                contact2.Email = tbxContactEmail2.Text;
                contact2.Mobile = tbxContactMobile2.Text;
                contact2.Save();
            }

            if (!newContact2.Visible && HDN_ContactID1.Value != "")
            {
                GarasERP.ClientContactPerson contact2 = new GarasERP.ClientContactPerson();
                if (contact2.LoadByPrimaryKey(long.Parse(HDN_ContactID1.Value)))
                {
                    contact2.MarkAsDeleted();
                    contact2.Save();
                }
            }

            if (newContact3.Visible && tbxContactName3.Text.Trim() != "" && tbxContactTitle3.Text.Trim() != "" && tbxContactMobile3.Text.Trim() != "")
            {
                GarasERP.ClientContactPerson contact3 = new GarasERP.ClientContactPerson();
                if (HDN_ConractID2.Value == "")
                {
                    contact3.AddNew();
                    contact3.CreatedBy = UserID;
                    contact3.CreationDate = DateTime.Now;
                }
                else
                    contact3.LoadByPrimaryKey(long.Parse(HDN_ConractID2.Value));
                
                contact3.ClientID = client.ID;
                contact3.ModifiedBy = UserID;
                contact3.Modified = DateTime.Now;
                contact3.Active = true;
                contact3.Name = tbxContactName3.Text;
                contact3.Title = tbxContactTitle3.Text;
                contact3.Location = tbxContactLocation3.Text;
                contact3.Email = tbxContactEmail3.Text;
                contact3.Mobile = tbxContactMobile3.Text;
                contact3.Save();
            }


            if (!newContact3.Visible && HDN_ConractID2.Value != "")
            {
                GarasERP.ClientContactPerson contact2 = new GarasERP.ClientContactPerson();
                if (contact2.LoadByPrimaryKey(long.Parse(HDN_ConractID2.Value)))
                {
                    contact2.MarkAsDeleted();
                    contact2.Save();
                }
            }

        }
        protected void Fill_Client_Specialty(GarasERP.Client client)
        {
            if (ddlSpeciality.Items != null && ddlSpeciality.SelectedValue!="")
            {
                GarasERP.ClientSpeciality clientSpecialty = new GarasERP.ClientSpeciality();
                if(HDN_SpecialityID.Value =="")
                {
                    clientSpecialty.AddNew();
                    clientSpecialty.CreatedBy = UserID;
                    clientSpecialty.CreationDate = DateTime.Now;
                }
                else
                {
                    clientSpecialty.LoadByPrimaryKey(long.Parse(HDN_SpecialityID.Value));
                }
               
                clientSpecialty.ClientID = client.ID;
                clientSpecialty.SpecialityID = int.Parse(ddlSpeciality.SelectedValue);
                clientSpecialty.ModifiedBy = UserID;
                clientSpecialty.Modified = DateTime.Now;
                clientSpecialty.Active = true;
                clientSpecialty.Save();
            }
           

            if (speciality2.Visible && ddlSpeciality2.Items != null && ddlSpeciality2.SelectedValue!="")
            {
                GarasERP.ClientSpeciality clientSpecialty2 = new GarasERP.ClientSpeciality();
                if (HDN_SpecialityID2.Value == "")
                {
                    clientSpecialty2.AddNew();
                    clientSpecialty2.CreatedBy = UserID;
                    clientSpecialty2.CreationDate = DateTime.Now;
                }
                else
                {
                    clientSpecialty2.LoadByPrimaryKey(long.Parse(HDN_SpecialityID2.Value));
                }
                clientSpecialty2.ClientID = client.ID;
                clientSpecialty2.SpecialityID = int.Parse(ddlSpeciality2.SelectedValue);
                clientSpecialty2.ModifiedBy = UserID;
                clientSpecialty2.Modified = DateTime.Now;
                clientSpecialty2.Active = true;
                clientSpecialty2.Save();
            }
            else
            {
                if (!speciality2.Visible && HDN_SpecialityID2.Value != "")
                {
                    GarasERP.ClientSpeciality clientSpecialty2 = new GarasERP.ClientSpeciality();
                    if(clientSpecialty2.LoadByPrimaryKey(long.Parse(HDN_SpecialityID2.Value)))
                    {
                        clientSpecialty2.MarkAsDeleted();
                        clientSpecialty2.Save();
                    }
                }
            }

            if (speciality3.Visible && ddlSpeciality3.Items != null && ddlSpeciality3.SelectedValue != "")
            {
                GarasERP.ClientSpeciality clientSpecialty3 = new GarasERP.ClientSpeciality();
                if (HDN_SpecialityID3.Value == "")
                {
                    clientSpecialty3.AddNew();
                    clientSpecialty3.CreatedBy = UserID;
                    clientSpecialty3.CreationDate = DateTime.Now;
                }
                else
                {
                    clientSpecialty3.LoadByPrimaryKey(long.Parse(HDN_SpecialityID3.Value));
                }
                
                clientSpecialty3.ClientID = client.ID;
                clientSpecialty3.SpecialityID = int.Parse(ddlSpeciality3.SelectedValue);
                clientSpecialty3.ModifiedBy = UserID;
                clientSpecialty3.Modified = DateTime.Now;
                clientSpecialty3.Active = true;
                clientSpecialty3.Save();
            }
            else
            {
                if (!speciality3.Visible && HDN_SpecialityID3.Value != "")
                {
                    GarasERP.ClientSpeciality clientSpecialty3 = new GarasERP.ClientSpeciality();
                    if (clientSpecialty3.LoadByPrimaryKey(long.Parse(HDN_SpecialityID3.Value)))
                    {
                        clientSpecialty3.MarkAsDeleted();
                        clientSpecialty3.Save();
                    }
                }
            }

            if (speciality4.Visible && ddlSpeciality4.Items != null && ddlSpeciality4.SelectedValue != "")
            {
                GarasERP.ClientSpeciality clientSpecialty4 = new GarasERP.ClientSpeciality();
                if (HDN_SpecialityID4.Value == "")
                {
                    clientSpecialty4.AddNew();
                    clientSpecialty4.CreatedBy = UserID;
                    clientSpecialty4.CreationDate = DateTime.Now;
                }
                else
                {
                    clientSpecialty4.LoadByPrimaryKey(long.Parse(HDN_SpecialityID4.Value));
                }
                clientSpecialty4.ClientID = client.ID;
                clientSpecialty4.SpecialityID = int.Parse(ddlSpeciality4.SelectedValue);
                clientSpecialty4.ModifiedBy = UserID;
                clientSpecialty4.Modified = DateTime.Now;
                clientSpecialty4.Active = true;
                clientSpecialty4.Save();
            }
            else
            {
                if (!speciality4.Visible && HDN_SpecialityID4.Value != "")
                {
                    GarasERP.ClientSpeciality clientSpecialty4 = new GarasERP.ClientSpeciality();
                    if (clientSpecialty4.LoadByPrimaryKey(long.Parse(HDN_SpecialityID4.Value)))
                    {
                        clientSpecialty4.MarkAsDeleted();
                        clientSpecialty4.Save();
                    }
                }
            }


        }

        protected void InsertClientConsultantData(GarasERP.Client client
            , GarasERP.ClientConsultant consultant
            , DataRow conslt)
        {
            //AddClientConsultantName(client, consultant);
            //AddClientConsultantAddress(consultant);
            //AddClientConsultantEmail(consultant);
            //AddClientConsultantPhone(consultant);
            //AddClientConsultantMobile(consultant);
            //AddClientConsultantFax(consultant);
            //AddClientConsultantSpeciality(consultant);
            AddClientConsultantName(client, consultant,conslt["Consult_ID"].ToString(), conslt["ConsltName"].ToString()
                                        , conslt["ConsltOffice"].ToString()
                                        , conslt["ConsltFor"].ToString());
            AddClientConsultantAddress(consultant, conslt["ConsltAddress_ID"].ToString()
                                    , conslt["ConsltCountry"].ToString()
                                    , conslt["ConsltGvrnrt"].ToString()
                                    , conslt["ConsltStreet"].ToString()
                                    , conslt["ConsltBuilding"].ToString()
                                    , conslt["ConsltFloor"].ToString()
                                    , conslt["ConsltDesc"].ToString());
            AddClientConsultantEmail(consultant, conslt["ConsltEmail_ID"].ToString()
                                    , conslt["ConsltEmail1_ID"].ToString()
                                    , conslt["ConsltEmail2_ID"].ToString()
                                    , conslt["ConsltEmail"].ToString()
                                    , conslt["ConsltEmail1"].ToString()
                                    , conslt["ConsltEmail2"].ToString());
            AddClientConsultantPhone(consultant, conslt["ConsltPhone_ID"].ToString()
                                    , conslt["ConsltPhone1_ID"].ToString()
                                    , conslt["ConsltPhone2_ID"].ToString()
                                    , conslt["ConsltPhone"].ToString()
                                    , conslt["ConsltPhone1"].ToString()
                                    , conslt["ConsltPhone2"].ToString());
            AddClientConsultantMobile(consultant, conslt["ConsltMobile_ID"].ToString()
                                    , conslt["ConsltMobile1_ID"].ToString()
                                    , conslt["ConsltMobile2_ID"].ToString()
                                    , conslt["ConsltMobile"].ToString()
                                    , conslt["ConsltMobile1"].ToString()
                                    , conslt["ConsltMobile2"].ToString());
            AddClientConsultantFax(consultant, conslt["ConsltFax_ID"].ToString()
                                    , conslt["ConsltFax1_ID"].ToString()
                                    , conslt["ConsltFax2_ID"].ToString()
                                    , conslt["ConsltFax"].ToString()
                                    , conslt["ConsltFax1"].ToString()
                                    , conslt["ConsltFax2"].ToString());
            AddClientConsultantSpeciality(consultant, conslt["ConsltSpecial1_ID"].ToString()
                                        , conslt["ConsltSpecial2_ID"].ToString()
                                        , conslt["ConsltSpecial3_ID"].ToString()
                                        , conslt["Special1"].ToString()
                                        , conslt["Special2"].ToString()
                                        , conslt["Special3"].ToString());
        }

        private void AddClientConsultantSpeciality(ClientConsultant consultant
                                                    , string ConsltSpecial_ID
                                                    , string ConsltSpecial
                                                    , string ConsltSpecial1_ID
                                                    , string ConsltSpecial1
                                                    , string ConsltSpecial2_ID
                                                    , string ConsltSpecial2)
        {
            if (!String.IsNullOrEmpty(ConsltSpecial))//DDL_ConsultantSpecialty.Items !=null && DDL_ConsultantSpecialty.SelectedValue!="")
            {
                ClientConsultantSpecialilty consultantSpicial = new GarasERP.ClientConsultantSpecialilty();
                if (String.IsNullOrEmpty(ConsltSpecial_ID))//HDN_ConsultantSpecialtyID.Value == "")
                {
                    consultantSpicial.AddNew();
                    consultantSpicial.CreatedBy = UserID;
                    consultantSpicial.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantSpicial.LoadByPrimaryKey(long.Parse(ConsltSpecial_ID));//HDN_ConsultantSpecialtyID.Value));
                }

                consultantSpicial.ConsultantID = consultant.ID;
                consultantSpicial.SpecialityID = int.Parse(ConsltSpecial);//DDL_ConsultantSpecialty.SelectedValue);
                consultantSpicial.ModifiedBy = UserID;
                consultantSpicial.Modified = DateTime.Now;
                consultantSpicial.Active = true;
                consultantSpicial.Save();
            }
            else
            {
                if (!String.IsNullOrEmpty(ConsltSpecial_ID))//HDN_ConsultantSpecialtyID.Value != "")
                {
                    ClientConsultantSpecialilty consultantSpicial = new GarasERP.ClientConsultantSpecialilty();
                    if (consultantSpicial.LoadByPrimaryKey(long.Parse(ConsltSpecial_ID)))// HDN_ConsultantSpecialtyID.Value)))
                    {
                        consultantSpicial.MarkAsDeleted();
                        consultantSpicial.Save();
                    }
                }
            }
            ///////////////////////////////////
            if (!string.IsNullOrEmpty(ConsltSpecial1))//speciality2.Visible && DDL_ConsultantSpecialty2.Items != null && DDL_ConsultantSpecialty2.SelectedValue != "")
            {
                ClientConsultantSpecialilty consultantSpicial = new GarasERP.ClientConsultantSpecialilty();
                if (String.IsNullOrEmpty(ConsltSpecial1_ID))//HDN_ConsultantSpecialtyID2.Value == "")
                {
                    consultantSpicial.AddNew();
                    consultantSpicial.CreatedBy = UserID;
                    consultantSpicial.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantSpicial.LoadByPrimaryKey(long.Parse(ConsltSpecial1_ID));// HDN_ConsultantSpecialtyID2.Value));
                }

                consultantSpicial.ConsultantID = consultant.ID;
                consultantSpicial.SpecialityID = int.Parse(ConsltSpecial1);// DDL_ConsultantSpecialty2.SelectedValue);
                consultantSpicial.ModifiedBy = UserID;
                consultantSpicial.Modified = DateTime.Now;
                consultantSpicial.Active = true;
                consultantSpicial.Save();
            }
            else
            {
                if (!String.IsNullOrEmpty(ConsltSpecial1_ID))//HDN_ConsultantSpecialtyID2.Value != "")
                {
                    ClientConsultantSpecialilty consultantSpicial = new GarasERP.ClientConsultantSpecialilty();
                    if (consultantSpicial.LoadByPrimaryKey(long.Parse(ConsltSpecial1_ID)))//HDN_ConsultantSpecialtyID2.Value)))
                    {
                        consultantSpicial.MarkAsDeleted();
                        consultantSpicial.Save();
                    }
                }
            }

            if (!String.IsNullOrEmpty(ConsltSpecial2))//speciality3.Visible &&ddlSpeciality3.Items != null && ddlSpeciality3.SelectedValue != "")
            {
                ClientConsultantSpecialilty consultantSpicial = new GarasERP.ClientConsultantSpecialilty();
                if (String.IsNullOrEmpty(ConsltSpecial2_ID))//HDN_SpecialityID3.Value == "")
                {
                    consultantSpicial.AddNew();
                    consultantSpicial.CreatedBy = UserID;
                    consultantSpicial.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantSpicial.LoadByPrimaryKey(long.Parse(ConsltSpecial2_ID));// HDN_SpecialityID3.Value));
                }

                consultantSpicial.ConsultantID = consultant.ID;
                consultantSpicial.SpecialityID = int.Parse(ConsltSpecial2);// ddlSpeciality3.SelectedValue);
                consultantSpicial.ModifiedBy = UserID;
                consultantSpicial.Modified = DateTime.Now;
                consultantSpicial.Active = true;
                consultantSpicial.Save();
            }
            else
            {
                if (!String.IsNullOrEmpty(ConsltSpecial2_ID))//HDN_SpecialityID3.Value != "")
                {
                    ClientConsultantSpecialilty consultantSpicial = new GarasERP.ClientConsultantSpecialilty();
                    if (consultantSpicial.LoadByPrimaryKey(long.Parse(ConsltSpecial2_ID)))//HDN_SpecialityID3.Value)))
                    {
                        consultantSpicial.MarkAsDeleted();
                        consultantSpicial.Save();
                    }
                }
            }
        }

        protected void AddClientConsultantName(GarasERP.Client client
            , GarasERP.ClientConsultant consultant
            ,string consltID, string consltName, string consltOffice, string selectedFor)
        {
            if(consltID == "")//HDN_ConsultantID.Value=="")
            {
                consultant.AddNew();
                consultant.CreatedBy = UserID;
                consultant.CreationDate = DateTime.Now;
            }
            else
            {
                consultant.LoadByPrimaryKey(long.Parse(consltID));// HDN_ConsultantID.Value));
            }
           
            consultant.ClientID = client.ID;
            consultant.ConsultantName = consltName;//tbxConsultantName.Text;
            consultant.Company = consltOffice;// tbxConsultantOffice.Text;
            consultant.ModifiedBy = UserID;
            consultant.Modified = DateTime.Now;
            consultant.Active = true;
            consultant.ConsultantFor = selectedFor;// DDl_For.SelectedValue;
            consultant.Save();
        }

        protected void AddClientConsultantAddress(GarasERP.ClientConsultant consultant
             , string consltAddressID, string consltCountry
             , string consltGovrnrt, string consltStreet
             , string consltBuilding, string consltFloor, string consltDesc)
        {
            GarasERP.ClientConsultantAddress consultantAddress = new GarasERP.ClientConsultantAddress();
            if (consltAddressID == "")//HDN_ddlConsultantAddressID.Value == "")
            {
                consultantAddress.AddNew();
                consultantAddress.CreatedBy = UserID;
                consultantAddress.CreationDate = DateTime.Now;
            }
            else
            {
                consultantAddress.LoadByPrimaryKey(long.Parse(consltAddressID/*HDN_ddlConsultantAddressID.Value*/));
            }
            
            consultantAddress.ConsultantID = consultant.ID;
            consultantAddress.CountryID = int.Parse(consltCountry/*ddlConsultantCountry.SelectedValue*/);
            consultantAddress.GovernorateID = int.Parse(consltGovrnrt/*ddlConsultantGovernorate.SelectedValue*/);
            consultantAddress.Address = consltStreet;//tbxConsultantStreet.Text;
            consultantAddress.BuildingNumber = consltBuilding;// tbxConsultantBuilding.Text;
            consultantAddress.Floor = consltFloor;// tbxConsultantFloor.Text;
            consultantAddress.Description = consltDesc;// tbxConsultantDescription.Text;
            consultantAddress.Active = true;
            consultantAddress.ModifiedBy = UserID;
            consultantAddress.Modified = DateTime.Now;
            consultantAddress.Save();

        }

        protected void AddClientConsultantEmail(GarasERP.ClientConsultant consultant
            , string email_ID, string email1_ID, string email2_ID
            , string email, string email1, string email2)
        {
            if (email.Trim() != "")//tbxConsultantEmail.Text.Trim() != "")
            {
                GarasERP.ClientConsultantEmail consultantEmail = new GarasERP.ClientConsultantEmail();
                if (email_ID == "")//HDN_ConsultantEmailID.Value == "")
                {
                    consultantEmail.AddNew();
                    consultantEmail.CreatedBy = UserID;
                    consultantEmail.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantEmail.LoadByPrimaryKey(long.Parse(email_ID/*HDN_ConsultantEmailID.Value*/));
                }
               
                consultantEmail.ConsultantID = consultant.ID;
                consultantEmail.Email = email;// tbxConsultantEmail.Text;
                consultantEmail.ModifiedBy = UserID;
                consultantEmail.Modified = DateTime.Now;
                consultantEmail.Active = true;
                consultantEmail.Save();
            }
            else
            {
                if (email_ID != "")//HDN_ConsultantEmailID.Value != "")
                {
                    GarasERP.ClientConsultantEmail consultantEmail = new GarasERP.ClientConsultantEmail();
                    if(consultantEmail.LoadByPrimaryKey(long.Parse(email_ID/*HDN_ConsultantEmailID.Value*/)))
                    {
                        consultantEmail.MarkAsDeleted();
                        consultantEmail.Save();
                    }
                }
            }


            if (email1.Trim() != "")//newConsultantEmail2.Visible && tbxConsultantEmail2.Text.Trim()!="")
            {
                GarasERP.ClientConsultantEmail consultantEmail2 = new GarasERP.ClientConsultantEmail();
                if (email1_ID == "")
                {
                    consultantEmail2.AddNew();
                    consultantEmail2.CreatedBy = UserID;
                    consultantEmail2.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantEmail2.LoadByPrimaryKey(long.Parse(email1_ID));
                }
                consultantEmail2.ConsultantID = consultant.ID;
                consultantEmail2.Email = email1;// tbxConsultantEmail2.Text;
                consultantEmail2.ModifiedBy = UserID;
                consultantEmail2.Modified = DateTime.Now;
                consultantEmail2.Active = true;
                consultantEmail2.Save();
            }
            else
            {
                if (email1_ID != "")
                {
                    GarasERP.ClientConsultantEmail consultantEmail2 = new GarasERP.ClientConsultantEmail();
                    if(consultantEmail2.LoadByPrimaryKey(long.Parse(email1_ID)))
                    {
                        consultantEmail2.MarkAsDeleted();
                        consultantEmail2.Save();
                    }
                }
            }

            if (email2.Trim() != "")//newConsultantEmail3.Visible && tbxConsultantEmail3.Text.Trim()!="")
            {
                GarasERP.ClientConsultantEmail consultantEmail3 = new GarasERP.ClientConsultantEmail();
                if (email2_ID == "")
                {
                    consultantEmail3.AddNew();
                    consultantEmail3.CreatedBy = UserID;
                    consultantEmail3.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantEmail3.LoadByPrimaryKey(long.Parse(email2_ID));
                }
                consultantEmail3.ConsultantID = consultant.ID;
                consultantEmail3.Email = email2;// tbxConsultantEmail3.Text;
                consultantEmail3.ModifiedBy = UserID;
                consultantEmail3.Modified = DateTime.Now;
                consultantEmail3.Active = true;
                consultantEmail3.Save();
            }
            else
            {
                if (email2_ID != "")
                {
                    GarasERP.ClientConsultantEmail consultantEmail3 = new GarasERP.ClientConsultantEmail();
                    if (consultantEmail3.LoadByPrimaryKey(long.Parse(email2_ID)))
                    {
                        consultantEmail3.MarkAsDeleted();
                        consultantEmail3.Save();
                    }
                }
            }
        }

        protected void AddClientConsultantPhone(GarasERP.ClientConsultant consultant
            , string consltPhone_ID, string consltPhone1_ID, string consltPhone2_ID
            , string consltPhone, string consltPhone1, string consltPhone2)
        {
            if (consltPhone.Trim() != "")
            {
                GarasERP.ClientConsultantPhone consultantPhone = new GarasERP.ClientConsultantPhone();
                if (consltPhone_ID == "")
                {
                    consultantPhone.AddNew();
                    consultantPhone.CreatedBy = UserID;
                    consultantPhone.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantPhone.LoadByPrimaryKey(long.Parse(consltPhone_ID));
                }
                
                consultantPhone.ConsultantID = consultant.ID;
                consultantPhone.Phone = consltPhone;// tbxConsultantPhone.Text;
                consultantPhone.ModifiedBy = UserID;
                consultantPhone.Modified = DateTime.Now;
                consultantPhone.Active = true;
                consultantPhone.Save();
            }
            else
            {
                if (consltPhone_ID != "")
                {
                    ClientConsultantPhone consultantPhone = new GarasERP.ClientConsultantPhone();
                    if(consultantPhone.LoadByPrimaryKey(long.Parse(consltPhone_ID)))
                    {
                        consultantPhone.MarkAsDeleted();
                        consultantPhone.Save();
                    }
                }
            }

            if (consltPhone1.Trim()!= "")//newConsultantPhone2.Visible && tbxConsultantPhone2.Text.Trim()!="")
            {
                GarasERP.ClientConsultantPhone consultantPhone2 = new GarasERP.ClientConsultantPhone();
                if (consltPhone1_ID == "")
                {
                    consultantPhone2.AddNew();
                    consultantPhone2.CreatedBy = UserID;
                    consultantPhone2.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantPhone2.LoadByPrimaryKey(long.Parse(consltPhone1_ID));
                }
                consultantPhone2.ConsultantID = consultant.ID;
                consultantPhone2.Phone = consltPhone1;// tbxConsultantPhone2.Text;
                consultantPhone2.ModifiedBy = UserID;
                consultantPhone2.Modified = DateTime.Now;
                consultantPhone2.Active = true;
                consultantPhone2.Save();
            }
            else
            {
                if (consltPhone1_ID != "")
                {
                    ClientConsultantPhone consultantPhone2 = new GarasERP.ClientConsultantPhone();
                    if (consultantPhone2.LoadByPrimaryKey(long.Parse(consltPhone1_ID)))
                    {
                        consultantPhone2.MarkAsDeleted();
                        consultantPhone2.Save();
                    }
                }
            }

            if (consltPhone2.Trim() !="")
            {
                GarasERP.ClientConsultantPhone consultantPhone3 = new GarasERP.ClientConsultantPhone();
                if (consltPhone2_ID == "")
                {
                    consultantPhone3.AddNew();
                    consultantPhone3.CreatedBy = UserID;
                    consultantPhone3.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantPhone3.LoadByPrimaryKey(long.Parse(consltPhone2_ID));
                }
                consultantPhone3.ConsultantID = consultant.ID;
                consultantPhone3.Phone = consltPhone2;// tbxConsultantPhone3.Text;
                consultantPhone3.ModifiedBy = UserID;
                consultantPhone3.Modified = DateTime.Now;
                consultantPhone3.Active = true;
                consultantPhone3.Save();
            }
            else
            {
                if (consltPhone2_ID != "")
                {
                    ClientConsultantPhone consultantPhone3 = new GarasERP.ClientConsultantPhone();
                    if (consultantPhone3.LoadByPrimaryKey(long.Parse(consltPhone2_ID)))
                    {
                        consultantPhone3.MarkAsDeleted();
                        consultantPhone3.Save();
                    }
                }
            }

        }

        protected void AddClientConsultantFax(GarasERP.ClientConsultant consultant
            , string consltFax_ID, string consltFax1_ID, string consltFax2_ID
            , string consltFax, string consltFax1, string consltFax2)
        {
            if (consltFax.Trim() != "")
            {
                GarasERP.ClientConsultantFax consultantFax = new GarasERP.ClientConsultantFax();
                if (consltFax_ID == "")
                {
                    consultantFax.AddNew();
                    consultantFax.CreatedBy = UserID;
                    consultantFax.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantFax.LoadByPrimaryKey(long.Parse(consltFax_ID));
                }
                consultantFax.ConsultantID = consultant.ID;
                consultantFax.Fax = consltFax;// tbxConsultantFax.Text;
                consultantFax.ModifiedBy = UserID;
                consultantFax.Modified = DateTime.Now;
                consultantFax.Active = true;
                consultantFax.Save();
            }
            else
            {
                if (consltFax_ID != "")
                {
                    ClientConsultantFax consultantFax = new GarasERP.ClientConsultantFax();
                    if (consultantFax.LoadByPrimaryKey(long.Parse(consltFax_ID)))
                    {
                        consultantFax.MarkAsDeleted();
                        consultantFax.Save();
                    }
                }
            }


            if (consltFax1.Trim()!="")
            {
                GarasERP.ClientConsultantFax consultantFax2 = new GarasERP.ClientConsultantFax();
                if (consltFax1_ID == "")
                {
                    consultantFax2.AddNew();
                    consultantFax2.CreatedBy = UserID;
                    consultantFax2.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantFax2.LoadByPrimaryKey(long.Parse(consltFax1_ID));
                }
                consultantFax2.ConsultantID = consultant.ID;
                consultantFax2.Fax = consltFax1;// tbxConsultantFax2.Text;
                consultantFax2.ModifiedBy = UserID;
                consultantFax2.Modified = DateTime.Now;
                consultantFax2.Active = true;
                consultantFax2.Save();
            }
            else
            {
                if (consltFax1_ID != "")
                {
                    ClientConsultantFax consultantFax2 = new GarasERP.ClientConsultantFax();
                    if (consultantFax2.LoadByPrimaryKey(long.Parse(consltFax1_ID)))
                    {
                        consultantFax2.MarkAsDeleted();
                        consultantFax2.Save();
                    }
                }
            }

            if (consltFax2.Trim()!="")
            {
                GarasERP.ClientConsultantFax consultantFax3 = new GarasERP.ClientConsultantFax();
                if (consltFax2_ID == "")
                {
                    consultantFax3.AddNew();
                    consultantFax3.CreatedBy = UserID;
                    consultantFax3.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantFax3.LoadByPrimaryKey(long.Parse(consltFax2_ID));
                }
                consultantFax3.ConsultantID = consultant.ID;
                consultantFax3.Fax = consltFax2;// tbxConsultantFax3.Text;
                consultantFax3.ModifiedBy = UserID;
                consultantFax3.Modified = DateTime.Now;
                consultantFax3.Active = true;
                consultantFax3.Save();
            }
            else
            {
                if (consltFax2_ID != "")
                {
                    ClientConsultantFax consultantFax3 = new GarasERP.ClientConsultantFax();
                    if (consultantFax3.LoadByPrimaryKey(long.Parse(consltFax2_ID)))
                    {
                        consultantFax3.MarkAsDeleted();
                        consultantFax3.Save();
                    }
                }
            }

        }
        
        protected void AddClientConsultantMobile(GarasERP.ClientConsultant consultant
            , string consltMobile_ID, string consltMobile1_ID, string consltMobile2_ID
            , string consltMobile, string consltMobile1, string consltMobile2)
        {
            if (consltMobile.Trim() != "")
            {
                GarasERP.ClientConsultantMobile consultantMobile = new GarasERP.ClientConsultantMobile();
                if (consltMobile_ID == "")
                {
                    consultantMobile.AddNew();
                    consultantMobile.CreatedBy = UserID;
                    consultantMobile.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantMobile.LoadByPrimaryKey(long.Parse(consltMobile_ID));
                }
                consultantMobile.ConsultantID = consultant.ID;
                consultantMobile.Mobile = consltMobile;// tbxConsultantMobile.Text;
                consultantMobile.CreatedBy = UserID;
                consultantMobile.CreationDate = DateTime.Now;
                consultantMobile.Active = true;
                consultantMobile.Save();
            }
            else
            {
                if (consltMobile_ID != "")
                {
                    ClientConsultantMobile consultantMobile = new GarasERP.ClientConsultantMobile();
                    if(consultantMobile.LoadByPrimaryKey(long.Parse(consltMobile_ID)))
                    {
                        consultantMobile.MarkAsDeleted();
                        consultantMobile.Save();
                    }
                }
            }

            if (consltMobile1.Trim()!="")
            {
                GarasERP.ClientConsultantMobile consultantMobile2 = new GarasERP.ClientConsultantMobile();
                if (consltMobile1_ID == "")
                {
                    consultantMobile2.AddNew();
                    consultantMobile2.CreatedBy = UserID;
                    consultantMobile2.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantMobile2.LoadByPrimaryKey(long.Parse(consltMobile1_ID));
                }
                consultantMobile2.ConsultantID = consultant.ID;
                consultantMobile2.Mobile = consltMobile1;// tbxConsultantMobile2.Text;
                consultantMobile2.ModifiedBy = UserID;
                consultantMobile2.Modified = DateTime.Now;
                consultantMobile2.Active = true;
                consultantMobile2.Save();
            }
            else
            {
                if (consltMobile1_ID != "")
                {
                    ClientConsultantMobile consultantMobile2 = new GarasERP.ClientConsultantMobile();
                    if (consultantMobile2.LoadByPrimaryKey(long.Parse(consltMobile1_ID)))
                    {
                        consultantMobile2.MarkAsDeleted();
                        consultantMobile2.Save();
                    }
                }
            }

            if (consltMobile2.Trim()!="")
            {
                GarasERP.ClientConsultantMobile consultantMobile3 = new GarasERP.ClientConsultantMobile();
                if (consltMobile2_ID == "")
                {
                    consultantMobile3.AddNew();
                    consultantMobile3.CreatedBy = UserID;
                    consultantMobile3.CreationDate = DateTime.Now;
                }
                else
                {
                    consultantMobile3.LoadByPrimaryKey(long.Parse(consltMobile2_ID));
                }
                consultantMobile3.ConsultantID = consultant.ID;
                consultantMobile3.Mobile = consltMobile2;// tbxConsultantMobile3.Text;
                consultantMobile3.ModifiedBy = UserID;
                consultantMobile3.Modified = DateTime.Now;
                consultantMobile3.Active = true;
                consultantMobile3.Save();
            }
            else
            {
                if (consltMobile2_ID != "")
                {
                    ClientConsultantMobile consultantMobile3 = new GarasERP.ClientConsultantMobile();
                    if (consultantMobile3.LoadByPrimaryKey(long.Parse(consltMobile2_ID)))
                    {
                        consultantMobile3.MarkAsDeleted();
                        consultantMobile3.Save();
                    }
                }
            }

        }


        protected void CreateAttachmentDirectory(GarasERP.Client client)
        {
            if (FileUpload1.HasFile || FileUpload2.HasFile || FileUpload3.HasFile)
            {
                var folder = Server.MapPath("~/Attachments/Clients/");
                var clientFolder = Server.MapPath("~/Attachments/Clients/" + client.ID + "/");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                if (!Directory.Exists(clientFolder))
                {
                    Directory.CreateDirectory(clientFolder);
                }

                if (FileUpload1.HasFile)
                {
                    GarasERP.ClientAttachment attachment = new GarasERP.ClientAttachment();
                    attachment.AddNew();
                    attachment.ClientID = client.ID;
                    attachment.CreatedBy = UserID;
                    attachment.CreationDate = DateTime.Now;
                    attachment.Active = true;
                    attachment.Type = "Business Cards";


                    string fileName = Path.GetFileName(FileUpload1.PostedFile.FileName);
                    string extension = fileName.Split('.').Last();
                    string Date = DateTime.Now.ToFileTime() + "_";
                    FileUpload1.PostedFile.SaveAs((clientFolder) + Date + fileName);
                    attachment.FileName = fileName;
                    attachment.AttachmentPath = (clientFolder) + Date + fileName;
                    if (attachment.AttachmentPath == DBNull.Value.ToString())
                        attachment.AttachmentPath = "N/A";
                    attachment.FileExtenssion = extension;
                    attachment.Save();

                    //Response.Redirect(Request.Url.AbsoluteUri);
                }
                if (FileUpload2.HasFile)
                {
                    GarasERP.ClientAttachment attachment = new GarasERP.ClientAttachment();
                    attachment.AddNew();
                    attachment.ClientID = client.ID;
                    attachment.CreatedBy = UserID;
                    attachment.CreationDate = DateTime.Now;
                    attachment.Active = true;
                    attachment.Type = "Brochure";
                    string fileName = Path.GetFileName(FileUpload2.PostedFile.FileName);
                    string extension = fileName.Split('.').Last();
                    string Date = DateTime.Now.ToFileTime() + "_";
                    FileUpload2.PostedFile.SaveAs((clientFolder) + Date + fileName);
                    attachment.FileName = fileName;
                    attachment.FileExtenssion = extension;

                    attachment.AttachmentPath = (clientFolder) + Date + fileName;
                    if (attachment.AttachmentPath == DBNull.Value.ToString())
                        attachment.AttachmentPath = "N/A";
                    attachment.Save();
                    //Response.Redirect(Request.Url.AbsoluteUri);
                }
                if (FileUpload3.HasFile)
                {
                    GarasERP.ClientAttachment attachment = new GarasERP.ClientAttachment();
                    attachment.AddNew();
                    attachment.ClientID = client.ID;
                    attachment.CreatedBy = UserID;
                    attachment.CreationDate = DateTime.Now;
                    attachment.Active = true;
                    attachment.Type = "Other";

                    string fileName = Path.GetFileName(FileUpload3.PostedFile.FileName);
                    string extension = fileName.Split('.').Last();
                    string Date = DateTime.Now.ToFileTime() + "_";
                    FileUpload3.PostedFile.SaveAs((clientFolder) + Date + fileName);
                    attachment.FileName = fileName;
                    attachment.FileExtenssion = extension;
                    attachment.AttachmentPath = (clientFolder) + Date + fileName;
                    if (attachment.AttachmentPath == DBNull.Value.ToString())
                        attachment.AttachmentPath = "N/A";
                    attachment.Save();
                    //Response.Redirect(Request.Url.AbsoluteUri);
                }

            }
        }
        #endregion


        protected void CreateNotification(GarasERP.Client client)
        {
            GarasERP.Notification notification = new GarasERP.Notification();
            notification.AddNew();
            notification.Title = "Client Updated And Assigned to you";
            notification.Description = client.Name + " is now assigned to you.";
            notification.Date = DateTime.Now;
            notification.New = true;
            notification.URL = EncryptRedirect(client.ID.ToString()); ;
            notification.UserID = long.Parse(ddlAssignedTo.SelectedValue);
            notification.Save();
        }
        public string EncryptRedirect(string clientID)
        {
            string url = "~/Sales/Client/ClientProfile.aspx?CID=";
            string queryStr = GarasERP.Encrypt_Decrypt.Encrypt(clientID, key);
            return url + Server.UrlEncode(queryStr);
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (GetUserPermission())
            {
                InsertClientData();
            }
            else
            {
                string script = "alert('Sorry, You donot have access to Add Clients. Please contact your Administrator!');";
                this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "Registration Failed", script, true /* addScriptTags */);
            }

            ////Clear textboxes after Registraion
            //CleartextBoxes(this);
        }

        protected bool GetUserPermission()
        {
            GarasERP.UserRole userRole = new GarasERP.UserRole();
            userRole.Where.UserID.Value = UserID;
            userRole.Where.RoleID.Conjuction = MyGeneration.dOOdads.WhereParameter.Conj.And;
            userRole.Where.RoleID.Value = 1;//RoleID = AddClient
            userRole.Query.Load();

            ////userRole.Where.RoleID.Conjuction = MyGeneration.dOOdads.WhereParameter.Conj.Or;
            //userRole.Where.RoleID.Value = 1;

            if (userRole.RowCount > 0)
            {
                if (userRole.RoleID == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                string GroupIDs = "";
                GarasERP.GroupRole groupRole = new GarasERP.GroupRole();
                groupRole.Where.RoleID.Value = 1;
                groupRole.Where.GroupID.Conjuction = MyGeneration.dOOdads.WhereParameter.Conj.And;

                GarasERP.Group_User groupUser = new GarasERP.Group_User();
                groupUser.Where.UserID.Value = UserID;
                if (groupUser.Query.Load())
                {
                    if (groupUser.DefaultView != null && groupUser.DefaultView.Count > 0)
                    {
                        do
                        {
                            if (GroupIDs == "")
                                GroupIDs = "'" + groupUser.GroupID + "'";
                            else
                                GroupIDs += ",'" + groupUser.GroupID + "'";

                        } while (groupUser.MoveNext());
                    }
                }

                groupRole.Where.GroupID.Value = GroupIDs;
                groupRole.Where.GroupID.Operator = MyGeneration.dOOdads.WhereParameter.Operand.In;
                if (GroupIDs != "")
                    groupRole.Query.Load();

                if (groupRole.RowCount > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }


        }

        protected bool IsHighLevelPermission()
        {
            bool result = false;

            GarasERP.Group_User group = new GarasERP.Group_User();
            group.Where.UserID.Value = UserID;
            group.Query.Load();

            if (group.RowCount > 0)
            {
                if (group.GroupID == 1 || group.GroupID == 2)
                {
                    result = true;
                }
                else
                    result = false;
            }

            return result;
        }

        public void CleartextBoxes(Control parent)
        {
            foreach (Control tbx in parent.Controls)
            {
                if ((tbx.GetType() == typeof(TextBox)))
                {
                    ((TextBox)(tbx)).Text = String.Empty;
                }
                if (tbx.HasControls())
                {
                    CleartextBoxes(tbx);
                }
            }
        }

        protected void ddlCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGovDDL();
            LoadArea();
        }

        protected void ddlCountry2_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGovDDL2();
            LoadArea2();
        }

        protected void ddlCountry3_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGovDDL3();
            LoadArea3();
        }


        #region Unwanted Parts *Commented*

        //protected void InitializeDataTable()
        //{
        //    dt = new DataTable();
        //    dt.Columns.Add(new DataColumn("MyColumn", typeof(string)));
        //    dt.AcceptChanges();
        //}
        //protected void lnk_Click(object sender, EventArgs e)
        //{
        //    DataRow dr = dt.NewRow();
        //    dr["MyColumn"] = "gggg";// ((TextBox)reptr.Controls[reptr.Controls.Count - 1].Controls[0].FindControl("tbx")).Text;
        //    dt.Rows.Add(dr);
        //    ViewState["DT"] = dt;
        //    //reptr.DataSource = dt;
        //    //reptr.DataBind();
        //}

        //protected void reptr_ItemCommand(object source, RepeaterCommandEventArgs e)
        //{

        //}

        //protected void clientPhoneRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        //{
        //    if (e.CommandName == "Add")
        //    {
        //        TextBox tbx = (TextBox)e.Item.FindControl("clientPhoneTbx");                              
        //        DataRow dr = dt.NewRow();
        //        dr["MyColumn"] = tbx.Text;// ((TextBox)reptr.Controls[reptr.Controls.Count - 1].Controls[0].FindControl("tbx")).Text;
        //        dt.Rows.Add(dr);
        //        ViewState["DT"] = dt;
        //        //clientPhoneRepeater.DataSource = dt;
        //        //clientPhoneRepeater.DataBind();
        //    }
        //}

        #endregion


        #region Add Extra Fields

        protected void btnAddSpeciality_Click(object sender, EventArgs e)
        {
            speciality2.Visible = true;
            btnAddSpeciality.Visible = false;
        }

        protected void btnRemoveSpeciality_Click(object sender, EventArgs e)
        {
            speciality2.Visible = false;
            btnAddSpeciality.Visible = true;
        }

        protected void btnAddSpeciality3_Click(object sender, EventArgs e)
        {
            speciality3.Visible = true;
            btnAddSpeciality3.Visible = false;
        }
        protected void btnRemoveSpeciality3_Click(object sender, EventArgs e)
        {
            speciality3.Visible = false;
            btnAddSpeciality3.Visible = true;
        }

        protected void btnRemoveSpeciality4_Click(object sender, EventArgs e)
        {
            speciality4.Visible = false;
            btnAddSpeciality4.Visible = true;

        }

        protected void btnAddSpeciality4_Click(object sender, EventArgs e)
        {
            speciality4.Visible = true;
            btnAddSpeciality4.Visible = false;
        }
        protected void btnAddNewAddress1_Click(object sender, EventArgs e)
        {
            address2.Visible = true;
            RFV_ddlCountry2.Enabled = true;
            RFV_ddlGovernate2.Enabled = true;
            pAddNewAddress1.Visible = false;

        }

        protected void btnRemoveAddress2_Click(object sender, EventArgs e)
        {
            address2.Visible = false;
            RFV_ddlCountry2.Enabled = false;
            RFV_ddlGovernate2.Enabled = false;
            pAddNewAddress1.Visible = true;
        }

        protected void btnAddNewAddress2_Click(object sender, EventArgs e)
        {
            address3.Visible = true;
            RFV_ddlCountry3.Enabled = true;
            RFV_ddlGovernate3.Enabled = true;
            pAddNewAddress2.Visible = false;
        }

        protected void btnRemoveAddress3_Click(object sender, EventArgs e)
        {
            address3.Visible = false;
            RFV_ddlCountry3.Enabled = false;
            RFV_ddlGovernate3.Enabled = false;
            pAddNewAddress2.Visible = true;
        }

        protected void btnAddNewPhone_Click(object sender, EventArgs e)
        {
            newPhone.Visible = true;
            btnAddNewPhone.Visible = false;
            RegularExpressionValidator6.Enabled = true;
        }

        protected void removeNewButton_Click(object sender, EventArgs e)
        {
            newPhone.Visible = false;
            btnAddNewPhone.Visible = true;
            RegularExpressionValidator6.Enabled = false;
        }

        protected void btnAddNewPhone2_Click(object sender, EventArgs e)
        {
            newPhone3.Visible = true;
            btnAddNewPhone2.Visible = false;
            RegularExpressionValidator7.Enabled = true;
        }

        protected void removeNewButton3_Click(object sender, EventArgs e)
        {
            newPhone3.Visible = false;
            btnAddNewPhone2.Visible = true;
            RegularExpressionValidator7.Enabled = false;
        }

        protected void btnAddNewMobile_Click(object sender, EventArgs e)
        {
            newMobile2.Visible = true;
            btnAddNewMobile.Visible = false;
            RegularExpressionValidator8.Enabled = true;
        }

        protected void btnRemoveMobile2_Click(object sender, EventArgs e)
        {
            newMobile2.Visible = false;
            btnAddNewMobile.Visible = true;
            RegularExpressionValidator8.Enabled = false;

        }

        protected void btnAddNewMobile3_Click(object sender, EventArgs e)
        {
            newMobile3.Visible = true;
            btnAddNewMobile3.Visible = false;
            RegularExpressionValidator9.Enabled = true;
        }

        protected void btnRemoveNewMobile3_Click(object sender, EventArgs e)
        {
            newMobile3.Visible = false;
            btnAddNewMobile3.Visible = true;
            RegularExpressionValidator9.Enabled = false;
        }

        protected void btnAddNewFax_Click(object sender, EventArgs e)
        {
            newFax2.Visible = true;
            btnAddNewFax.Visible = false;
            RegularExpressionValidator10.Enabled = true;
        }

        protected void btnRemoveFax2_Click(object sender, EventArgs e)
        {
            newFax2.Visible = false;
            btnAddNewFax.Visible = true;
            RegularExpressionValidator10.Enabled = false;

        }

        protected void btnAddNewFax3_Click(object sender, EventArgs e)
        {
            newFax3.Visible = true;
            btnAddNewFax3.Visible = false;
            RegularExpressionValidator11.Enabled = true;
        }

        protected void btnRemoveFax3_Click(object sender, EventArgs e)
        {
            newFax3.Visible = false;
            btnAddNewFax3.Visible = true;
            RegularExpressionValidator11.Enabled = false;

        }

        protected void btnAddNewContact_Click(object sender, EventArgs e)
        {
            newContact2.Visible = true;
            pAddNewContact.Visible = false;
            RequiredFieldValidator3.Enabled = true;
            RequiredFieldValidator4.Enabled = true;
            RequiredFieldValidator5.Enabled = true;

        }

        protected void btnRemoveContact_Click(object sender, EventArgs e)
        {
            newContact2.Visible = false;
            pAddNewContact.Visible = true;
            RequiredFieldValidator3.Enabled = false;
            RequiredFieldValidator4.Enabled = false;
            RequiredFieldValidator5.Enabled = false;
        }
        protected void btnAddNewContact2_Click(object sender, EventArgs e)
        {
            newContact3.Visible = true;
            pAddNewContact2.Visible = false;
            RequiredFieldValidator6.Enabled = true;
            RequiredFieldValidator7.Enabled = true;
            RequiredFieldValidator8.Enabled = true;
        }

        protected void btnRemoveContact3_Click(object sender, EventArgs e)
        {
            newContact3.Visible = false;
            pAddNewContact2.Visible = true;
            RequiredFieldValidator6.Enabled = false;
            RequiredFieldValidator7.Enabled = false;
            RequiredFieldValidator8.Enabled = false;
        }

        //protected void btnAddConsultantEmail_Click(object sender, EventArgs e)
        //{
        //    newConsultantEmail2.Visible = true;
        //    btnAddConsultantEmail.Visible = false;
        //}

        //protected void btnRemoveConsultantEmail_Click(object sender, EventArgs e)
        //{
        //    newConsultantEmail2.Visible = false;
        //    btnAddConsultantEmail.Visible = true;
        //}

        //protected void btnAddConsultantEmail3_Click(object sender, EventArgs e)
        //{
        //    newConsultantEmail3.Visible = true;
        //    btnAddConsultantEmail3.Visible = false;
        //}

        //protected void btnRemoveConsultantEmail3_Click(object sender, EventArgs e)
        //{
        //    newConsultantEmail3.Visible = false;
        //    btnAddConsultantEmail3.Visible = true;
        //}

        //protected void btnAddNewConsultantPhone_Click(object sender, EventArgs e)
        //{
        //    newConsultantPhone2.Visible = true;
        //    btnAddNewConsultantPhone.Visible = false;

        //}

        //protected void btnRemoveConsultantPhone2_Click(object sender, EventArgs e)
        //{
        //    newConsultantPhone2.Visible = false;
        //    btnAddNewConsultantPhone.Visible = true;
        //}

        //protected void btnAddNewConsultantPhone2_Click(object sender, EventArgs e)
        //{
        //    newConsultantPhone3.Visible = true;
        //    btnAddNewConsultantPhone2.Visible = false;

        //}

        //protected void btnRemoveConsultantPhone3_Click(object sender, EventArgs e)
        //{
        //    newConsultantPhone3.Visible = false;
        //    btnAddNewConsultantPhone2.Visible = true;
        //}


        //protected void btnAddNewConsultantMobile_Click(object sender, EventArgs e)
        //{
        //    newConsultantMobile2.Visible = true;
        //    btnAddNewConsultantMobile.Visible = false;
        //}

        //protected void btnRemoveConsultantMobile2_Click(object sender, EventArgs e)
        //{
        //    newConsultantMobile2.Visible = false;
        //    btnAddNewConsultantMobile.Visible = true;
        //}

        //protected void btnAddNewConsultantMobile3_Click(object sender, EventArgs e)
        //{
        //    newConsultantMobile3.Visible = true;
        //    btnAddNewConsultantMobile3.Visible = false;
        //}

        //protected void btnRemoveConsultantMobile3_Click(object sender, EventArgs e)
        //{
        //    newConsultantMobile3.Visible = false;
        //    btnAddNewConsultantMobile3.Visible = true;
        //}

        //protected void btnAddConsultantFax_Click(object sender, EventArgs e)
        //{
        //    newConsultantFax2.Visible = true;
        //    btnAddConsultantFax.Visible = false;
        //}

        //protected void btnRemoveConsultantFax2_Click(object sender, EventArgs e)
        //{
        //    newConsultantFax2.Visible = false;
        //    btnAddConsultantFax.Visible = true;
        //}

        //protected void btnAddConsultantFax3_Click(object sender, EventArgs e)
        //{
        //    newConsultantFax3.Visible = true;
        //    btnAddConsultantFax3.Visible = false;
        //}

        //protected void btnRemoveConsultantFax3_Click(object sender, EventArgs e)
        //{
        //    newConsultantFax3.Visible = false;
        //    btnAddConsultantFax3.Visible = true;
        //}


        #endregion



        //protected void btnAddConsultantSpeciality_Click(object sender, EventArgs e)
        //{
        //    btnAddConsultantSpeciality.Visible = false;
        //    consultantSpeciality2.Visible = true;

        //}

        //protected void btnRemoveConsultantSpeciality_Click(object sender, EventArgs e)
        //{
        //    btnAddConsultantSpeciality.Visible = true;
        //    consultantSpeciality2.Visible = false;
        //}
        //protected void btnAddConsultantSpeciality2_Click(object sender, EventArgs e)
        //{
        //    btnAddConsultantSpeciality2.Visible = false;
        //    consultantSpeciality3.Visible = true;
        //}

        //protected void btnRemoveConsultantSpecialty3_Click(object sender, EventArgs e)
        //{
        //    btnAddConsultantSpeciality2.Visible = true;
        //    consultantSpeciality3.Visible = false;
        //}

        protected void CHB_SupportedByCompany_CheckedChanged(object sender, EventArgs e)
        {
            DDL_Supported.Visible = CHB_SupportedByCompany.Checked;
        }

        protected void ddlGovernate_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadArea();
        }

        protected void ddlGovernate2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadArea2();
        }

        protected void ddlGovernate3_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadArea3();
        }

        protected void rptrConsaltant_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                if (e.Item.ItemType == ListItemType.Item
                    || e.Item.ItemType == ListItemType.AlternatingItem)
                {
                    DataRowView consultRow = ((DataRowView)e.Item.DataItem);
                    DropDownList DDl_For = ((DropDownList)e.Item.FindControl("DDl_For"));
                    DropDownList ddlConsultantCountry = ((DropDownList)e.Item.FindControl("ddlConsultantCountry"));
                    DropDownList ddlConsultantGovernorate = ((DropDownList)e.Item.FindControl("ddlConsultantGovernorate"));
                    //RadioButton rbConsultantCompany = (RadioButton)e.Item.FindControl("rbConsultantCompany");
                    HtmlGenericControl consultantFirstDiv = (HtmlGenericControl)e.Item.FindControl("consultantFirstDiv");
                    HtmlGenericControl consultantSecondDiv = (HtmlGenericControl)e.Item.FindControl("consultantSecondDiv");
                    Label lblConsultantOffice = (Label)e.Item.FindControl("lblConsultantOffice");
                    TextBox tbxConsultantOffice = (TextBox)e.Item.FindControl("tbxConsultantOffice");
                    Label lblConsultantName = (Label)e.Item.FindControl("lblConsultantName");
                    TextBox tbxConsultantName = (TextBox)e.Item.FindControl("tbxConsultantName");
                    //RadioButton rbConsultantIndiv = (RadioButton)e.Item.FindControl("rbConsultantIndiv");
                    DropDownList DDL_ConsultantSpecialty = ((DropDownList)e.Item.FindControl("DDL_ConsultantSpecialty"));
                    DropDownList DDL_ConsultantSpecialty2 = ((DropDownList)e.Item.FindControl("DDL_ConsultantSpecialty2"));
                    DropDownList DDL_ConsultantSpeciality3 = ((DropDownList)e.Item.FindControl("DDL_ConsultantSpeciality3"));
                    TextBox tbxConsultantEmail = (TextBox)e.Item.FindControl("tbxConsultantEmail");
                    TextBox tbxConsultantEmail2 = (TextBox)e.Item.FindControl("tbxConsultantEmail2");
                    TextBox tbxConsultantEmail3 = (TextBox)e.Item.FindControl("tbxConsultantEmail3");
                    TextBox tbxConsultantPhone = (TextBox)e.Item.FindControl("tbxConsultantPhone");
                    TextBox tbxConsultantPhone2 = (TextBox)e.Item.FindControl("tbxConsultantPhone2");
                    TextBox tbxConsultantPhone3 = (TextBox)e.Item.FindControl("tbxConsultantPhone3");
                    TextBox tbxConsultantMobile = (TextBox)e.Item.FindControl("tbxConsultantMobile");
                    TextBox tbxConsultantMobile2 = (TextBox)e.Item.FindControl("tbxConsultantMobile2");
                    TextBox tbxConsultantMobile3 = (TextBox)e.Item.FindControl("tbxConsultantMobile3");
                    TextBox tbxConsultantFax = (TextBox)e.Item.FindControl("tbxConsultantFax");
                    TextBox tbxConsultantFax2 = (TextBox)e.Item.FindControl("tbxConsultantFax2");
                    TextBox tbxConsultantFax3 = (TextBox)e.Item.FindControl("tbxConsultantFax3");
                    TextBox tbxConsultantStreet = (TextBox)e.Item.FindControl("tbxConsultantStreet");
                    TextBox tbxConsultantBuilding = (TextBox)e.Item.FindControl("tbxConsultantBuilding");
                    TextBox tbxConsultantFloor = (TextBox)e.Item.FindControl("tbxConsultantFloor");
                    TextBox tbxConsultantDescription = (TextBox)e.Item.FindControl("tbxConsultantDescription");


                    DDL_ConsultantSpecialty.DataSource = VS_Speciality;
                    DDL_ConsultantSpecialty.DataTextField = GarasERP.Speciality.ColumnNames.Name;
                    DDL_ConsultantSpecialty.DataValueField = GarasERP.Speciality.ColumnNames.ID;
                    DDL_ConsultantSpecialty.DataBind();

                    DDL_ConsultantSpecialty2.DataSource = VS_Speciality;
                    DDL_ConsultantSpecialty2.DataTextField = GarasERP.Speciality.ColumnNames.Name;
                    DDL_ConsultantSpecialty2.DataValueField = GarasERP.Speciality.ColumnNames.ID;
                    DDL_ConsultantSpecialty2.DataBind();

                    DDL_ConsultantSpeciality3.DataSource = VS_Speciality;
                    DDL_ConsultantSpeciality3.DataTextField = GarasERP.Speciality.ColumnNames.Name;
                    DDL_ConsultantSpeciality3.DataValueField = GarasERP.Speciality.ColumnNames.ID;
                    DDL_ConsultantSpeciality3.DataBind();

                    BindConsGovDDL(ddlConsultantCountry, ddlConsultantGovernorate);

                    ddlConsultantCountry.DataSource = VS_Country;
                    ddlConsultantCountry.DataTextField = GarasERP.Country.ColumnNames.Name;
                    ddlConsultantCountry.DataValueField = GarasERP.Country.ColumnNames.ID;
                    ddlConsultantCountry.DataBind();
                    tbxConsultantOffice.Text = consultRow["ConsltOffice"].ToString();
                    tbxConsultantName.Text = consultRow["ConsltName"].ToString();
                    DDL_ConsultantSpecialty.SelectedValue = consultRow["Special1"].ToString();
                    DDl_For.SelectedValue = consultRow["ConsltFor"].ToString();
                    DDL_ConsultantSpecialty2.SelectedValue = consultRow["Special2"].ToString();
                    DDL_ConsultantSpeciality3.SelectedValue = consultRow["Special3"].ToString();
                    tbxConsultantEmail.Text = consultRow["ConsltEmail"].ToString();
                    tbxConsultantEmail2.Text = consultRow["ConsltEmail1"].ToString();
                    tbxConsultantEmail3.Text = consultRow["ConsltEmail2"].ToString();
                    tbxConsultantPhone.Text = consultRow["ConsltPhone"].ToString();
                    tbxConsultantPhone2.Text = consultRow["ConsltPhone1"].ToString();
                    tbxConsultantPhone3.Text = consultRow["ConsltPhone2"].ToString();
                    tbxConsultantMobile.Text = consultRow["ConsltMobile"].ToString();
                    tbxConsultantMobile2.Text = consultRow["ConsltMobile1"].ToString();
                    tbxConsultantMobile3.Text = consultRow["ConsltMobile2"].ToString();
                    tbxConsultantFax.Text = consultRow["ConsltFax"].ToString();
                    tbxConsultantFax2.Text = consultRow["ConsltFax1"].ToString();
                    tbxConsultantFax3.Text = consultRow["ConsltFax2"].ToString();
                    ddlConsultantCountry.SelectedValue = consultRow["ConsltCountry"].ToString();
                    ddlConsultantGovernorate.SelectedValue = consultRow["ConsltGvrnrt"].ToString();
                    tbxConsultantStreet.Text = consultRow["ConsltStreet"].ToString();
                    tbxConsultantBuilding.Text = consultRow["ConsltBuilding"].ToString();
                    tbxConsultantFloor.Text = consultRow["ConsltFloor"].ToString();
                    tbxConsultantDescription.Text = consultRow["ConsltDesc"].ToString();
                    //if (rbConsultantCompany.Checked == true)
                    //{
                    consultantFirstDiv.Visible = true;
                    consultantSecondDiv.Visible = true;

                    lblConsultantOffice.Visible = true;
                    tbxConsultantOffice.Visible = true;

                    lblConsultantName.Visible = true;
                    tbxConsultantName.Visible = true;
                    //}
                    //else if (rbConsultantIndiv.Checked == true)
                    //{
                    //    consultantFirstDiv.Visible = true;
                    //    consultantSecondDiv.Visible = true;
                    //    //consultantContacts.Visible = true;

                    //    lblConsultantOffice.Visible = false;
                    //    tbxConsultantOffice.Visible = false;

                    //    lblConsultantName.Visible = true;
                    //    tbxConsultantName.Visible = true;
                    //}
                }

            }
            catch (Exception ex)
            {
                //log error
            }
        }

        private void bindConsultFooter()
        {
            //(rptrConsaltant.Controls[rptrConsaltant.Controls.Count - 1].Controls[0].FindControl("lblTotal") as Label).Text = totalMarks.ToString();
        }

        protected void rptrConsaltant_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            //Button button = (source as Button);
            RepeaterItem item = e.Item;//button.NamingContainer as RepeaterItem;
            HtmlGenericControl consultantSpeciality2 = (HtmlGenericControl)item.FindControl("consultantSpeciality2");
            Button btnAddConsultantSpeciality2 = (Button)item.FindControl("btnAddConsultantSpeciality2");
            HtmlGenericControl consultantSpeciality3 = (HtmlGenericControl)item.FindControl("consultantSpeciality3");
            Button btnAddConsultantEmail = (Button)item.FindControl("btnAddConsultantEmail");
            HtmlGenericControl newConsultantEmail2 = (HtmlGenericControl)item.FindControl("newConsultantEmail2");
            Button btnAddConsultantEmail3 = (Button)item.FindControl("btnAddConsultantEmail3");
            HtmlGenericControl newConsultantEmail3 = (HtmlGenericControl)item.FindControl("newConsultantEmail3");
            Button btnAddNewConsultantPhone = (Button)item.FindControl("btnAddNewConsultantPhone");
            HtmlGenericControl newConsultantPhone2 = (HtmlGenericControl)item.FindControl("newConsultantPhone2");
            Button btnAddNewConsultantPhone2 = (Button)item.FindControl("btnAddNewConsultantPhone2");
            HtmlGenericControl newConsultantPhone3 = (HtmlGenericControl)item.FindControl("newConsultantPhone3");
            Button btnAddNewConsultantMobile = (Button)item.FindControl("btnAddNewConsultantMobile");
            HtmlGenericControl newConsultantMobile2 = (HtmlGenericControl)item.FindControl("newConsultantMobile2");
            Button btnAddNewConsultantMobile3 = (Button)item.FindControl("btnAddNewConsultantMobile3");
            HtmlGenericControl newConsultantMobile3 = (HtmlGenericControl)item.FindControl("newConsultantMobile3");
            Button btnAddConsultantFax = (Button)item.FindControl("btnAddConsultantFax");
            HtmlGenericControl newConsultantFax2 = (HtmlGenericControl)item.FindControl("newConsultantFax2");
            Button btnAddConsultantFax3 = (Button)item.FindControl("btnAddConsultantFax3");
            HtmlGenericControl newConsultantFax3 = (HtmlGenericControl)item.FindControl("newConsultantFax3");

            //DropDownList DDL_ConsultantSpecialty = ((DropDownList)item.FindControl("DDL_ConsultantSpecialty"));
            //DropDownList DDL_ConsultantSpecialty2 = ((DropDownList)item.FindControl("DDL_ConsultantSpecialty2"));
            //DropDownList DDL_ConsultantSpeciality3 = ((DropDownList)item.FindControl("DDL_ConsultantSpeciality3"));
            //TextBox tbxConsultantEmail = (TextBox)item.FindControl("tbxConsultantEmail");
            TextBox tbxConsultantEmail2 = (TextBox)item.FindControl("tbxConsultantEmail2");
            TextBox tbxConsultantEmail3 = (TextBox)item.FindControl("tbxConsultantEmail3");
            //TextBox tbxConsultantPhone = (TextBox)item.FindControl("tbxConsultantPhone");
            TextBox tbxConsultantPhone2 = (TextBox)item.FindControl("tbxConsultantPhone2");
            TextBox tbxConsultantPhone3 = (TextBox)item.FindControl("tbxConsultantPhone3");
            //TextBox tbxConsultantMobile = (TextBox)item.FindControl("tbxConsultantMobile");
            TextBox tbxConsultantMobile2 = (TextBox)item.FindControl("tbxConsultantMobile2");
            TextBox tbxConsultantMobile3 = (TextBox)item.FindControl("tbxConsultantMobile3");
            //TextBox tbxConsultantFax = (TextBox)item.FindControl("tbxConsultantFax");
            TextBox tbxConsultantFax2 = (TextBox)item.FindControl("tbxConsultantFax2");
            TextBox tbxConsultantFax3 = (TextBox)item.FindControl("tbxConsultantFax3");



            switch (e.CommandName)
            {
                case "AddConsultantSpeciality":
                    consultantSpeciality2.Visible = true;
                    break;
                case "RemoveConsultantSpeciality":
                    consultantSpeciality2.Visible = false;
                    break;
                case "AddConsultantSpeciality2":
                    btnAddConsultantSpeciality2.Visible = false;
                    consultantSpeciality3.Visible = true;
                    break;
                case "RemoveConsultantSpecialty3":
                    btnAddConsultantSpeciality2.Visible = true;
                    consultantSpeciality3.Visible = false;
                    break;
                case "AddConsultantEmail":
                    newConsultantEmail2.Visible = true;
                    btnAddConsultantEmail.Visible = false;
                    break;
                case "RemoveConsultantEmail":
                    tbxConsultantEmail2.Text = "";
                    newConsultantEmail2.Visible = false;
                    btnAddConsultantEmail.Visible = true;
                    break;
                case "AddConsultantEmail3":
                    newConsultantEmail3.Visible = true;
                    btnAddConsultantEmail3.Visible = false;
                    break;
                case "RemoveConsultantEmail3":
                    tbxConsultantEmail3.Text = "";
                    newConsultantEmail3.Visible = false;
                    btnAddConsultantEmail3.Visible = true;
                    break;
                case "AddNewConsultantPhone":
                    newConsultantPhone2.Visible = true;
                    btnAddNewConsultantPhone.Visible = false;
                    break;
                case "RemoveConsultantPhone2":
                    tbxConsultantPhone2.Text = "";
                    newConsultantPhone2.Visible = false;
                    btnAddNewConsultantPhone.Visible = true;
                    break;
                case "AddNewConsultantPhone2":
                    newConsultantPhone3.Visible = true;
                    btnAddNewConsultantPhone2.Visible = false;
                    break;
                case "RemoveConsultantPhone3":
                    tbxConsultantPhone3.Text = "";
                    newConsultantPhone3.Visible = false;
                    btnAddNewConsultantPhone2.Visible = true;
                    break;
                case "AddNewConsultantMobile":
                    newConsultantMobile2.Visible = true;
                    btnAddNewConsultantMobile.Visible = false;
                    break;
                case "RemoveConsultantMobile2":
                    tbxConsultantMobile2.Text = "";
                    newConsultantMobile2.Visible = false;
                    btnAddNewConsultantMobile.Visible = true;
                    break;
                case "AddNewConsultantMobile3":
                    newConsultantMobile3.Visible = true;
                    btnAddNewConsultantMobile3.Visible = false;
                    break;
                case "RemoveConsultantMobile3":
                    tbxConsultantMobile3.Text = "";
                    newConsultantMobile3.Visible = false;
                    btnAddNewConsultantMobile3.Visible = true;
                    break;
                case "AddConsultantFax":
                    newConsultantFax2.Visible = true;
                    btnAddConsultantFax.Visible = false;
                    break;
                case "RemoveConsultantFax2":
                    tbxConsultantFax2.Text = "";
                    newConsultantFax2.Visible = false;
                    btnAddConsultantFax.Visible = true;
                    break;
                case "AddConsultantFax3":
                    newConsultantFax3.Visible = true;
                    btnAddConsultantFax3.Visible = false;
                    break;
                case "RemoveConsultantFax3":
                    tbxConsultantFax3.Text = "";
                    newConsultantFax3.Visible = false;
                    btnAddConsultantFax3.Visible = true;
                    break;
                case "AddConsultant":
                    addItemToVS_Consultant(e.Item);
                    rptrConsaltant.DataSource = VS_Consultant;
                    rptrConsaltant.DataBind();
                    break;
                case "RemoveConsultant":
                    DataTable dt = VS_Consultant;
                    dt.Rows.RemoveAt(item.ItemIndex);
                    VS_Consultant = dt;
                    rptrConsaltant.DataSource = VS_Consultant;
                    rptrConsaltant.DataBind();
                    break;
            }
        }

        private void addItemToVS_Consultant(RepeaterItem item)
        {
            DataTable dt = VS_Consultant;
            DataRow dr = dt.NewRow();
            dr["ConsltName"] = ((TextBox)item.FindControl("tbxConsultantName")).Text;
            dr["ConsltOffice"] = ((TextBox)item.FindControl("tbxConsultantOffice")).Text;
            dr["ConsltFor"] = ((DropDownList)item.FindControl("DDl_For")).SelectedValue;
            dr["ConsltCountry"] = ((DropDownList)item.FindControl("ddlConsultantCountry")).SelectedValue;
            dr["ConsltGvrnrt"] = ((DropDownList)item.FindControl("ddlConsultantGovernorate")).SelectedValue;
            dr["ConsltStreet"] = ((TextBox)item.FindControl("tbxConsultantStreet")).Text;
            dr["ConsltBuilding"] = ((TextBox)item.FindControl("tbxConsultantBuilding")).Text;
            dr["ConsltFloor"] = ((TextBox)item.FindControl("tbxConsultantFloor")).Text;
            dr["ConsltDesc"] = ((TextBox)item.FindControl("tbxConsultantDescription")).Text;
            dr["ConsltEmail"] = ((TextBox)item.FindControl("tbxConsultantEmail")).Text;
            dr["ConsltEmail1"] = ((TextBox)item.FindControl("tbxConsultantEmail2")).Text;
            dr["ConsltEmail2"] = ((TextBox)item.FindControl("tbxConsultantEmail3")).Text;
            dr["ConsltPhone"] = ((TextBox)item.FindControl("tbxConsultantPhone")).Text;
            dr["ConsltPhone1"] = ((TextBox)item.FindControl("tbxConsultantPhone2")).Text;
            dr["ConsltPhone2"] = ((TextBox)item.FindControl("tbxConsultantPhone3")).Text;
            dr["ConsltMobile"] = ((TextBox)item.FindControl("tbxConsultantMobile")).Text;
            dr["ConsltMobile1"] = ((TextBox)item.FindControl("tbxConsultantMobile2")).Text;
            dr["ConsltMobile2"] = ((TextBox)item.FindControl("tbxConsultantMobile3")).Text;
            dr["ConsltFax"] = ((TextBox)item.FindControl("tbxConsultantFax")).Text;
            dr["ConsltFax1"] = ((TextBox)item.FindControl("tbxConsultantFax2")).Text;
            dr["ConsltFax2"] = ((TextBox)item.FindControl("tbxConsultantFax3")).Text;
            dr["Special1"] = ((DropDownList)item.FindControl("DDL_ConsultantSpecialty")).SelectedValue;
            dr["Special2"] = ((DropDownList)item.FindControl("DDL_ConsultantSpecialty2")).SelectedValue;
            dr["Special3"] = ((DropDownList)item.FindControl("ddlSpeciality3")).SelectedValue;
            dt.Rows.Add(dr);
            VS_Consultant = dt;
        }

    }
}