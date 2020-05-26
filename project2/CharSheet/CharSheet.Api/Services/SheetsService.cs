using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CharSheet.Domain;
using CharSheet.Data;
using CharSheet.Api.Models;

namespace CharSheet.Api.Services
{
    public partial interface IBusinessService
    {
        Task<IEnumerable<SheetModel>> GetSheets(object id);
        Task<SheetModel> GetSheet(object id);
        Task<FormInputGroupModel> GetFormInputGroup(FormInputGroup formInputGroup);
        Task<FormInputGroupModel> GetFormInputGroup(object id);
    }

    public partial class BusinessService : IBusinessService
    {
        #region GET
        public async Task<IEnumerable<SheetModel>> GetSheets(object id)
        {
            // Load sheets from database, filter by user id.
            var sheets = await _unitOfWork.SheetRepository.Get(sheet => sheet.UserId == (Guid)id);

            // Sheets as sheet models.
            var sheetModels = new List<SheetModel>();
            foreach (var sheet in sheets)
            {
                // Sheet as sheet model.
                var sheetModel = await ToModel(sheet);
            }

            return sheetModels.AsEnumerable();
        }

        public async Task<SheetModel> GetSheet(object id)
        {
            // Load sheet from database.
            var sheet = await _unitOfWork.SheetRepository.Find(id);
            if (sheet == null)
                throw new InvalidOperationException("Sheet not found");

            // Returns sheet as sheet model.
            return await ToModel(sheet);
        }

        public async Task<FormInputGroupModel> GetFormInputGroup(FormInputGroup formInputGroup)
        {
            // Instantiate form input group model.
            var id = formInputGroup.FormInputGroupId;
            var formInputGroupModel = new FormInputGroupModel
            {
                FormInputGroupId = formInputGroup.FormInputGroupId,

                // Get form template as model.
                FormTemplate = await this.GetFormTemplate(formInputGroup.FormTemplateId)
            };

            // Get form inputs as model.
            var formInputs = await _unitOfWork.FormInputGroupRepository.GetFormInputs(id);
            formInputGroupModel.FormInputs = formInputs.Select(formInput => formInput.Value);
            return formInputGroupModel;
        }

        public async Task<FormInputGroupModel> GetFormInputGroup(object id)
        {
            // Load form input group from database.
            var formInputGroup = await _unitOfWork.FormInputGroupRepository.Find(id);
            if (formInputGroup == null)
                throw new InvalidOperationException("Form input group not found.");
            return await GetFormInputGroup(formInputGroup);
        }
        #endregion

        #region Helpers
        public async Task<SheetModel> ToModel(Sheet sheet)
        {
            // Load form input groups.
            var formInputGroups = await _unitOfWork.SheetRepository.GetFormInputGroups(sheet.SheetId);

            // Instantiate sheet model.
            var sheetModel = new SheetModel
            {
                SheetId = sheet.SheetId,
                UserId = sheet.UserId,
            };

            // Instantiate a form input group model for each form input group.
            var formInputGroupModels = new List<FormInputGroupModel>();
            foreach (var formInputGroup in formInputGroups)
            {
                // Convert form input group object to model.
                var formInputGroupModel = await GetFormInputGroup(formInputGroup);
                formInputGroupModels.Add(formInputGroupModel);
            }
            sheetModel.FormGroups = formInputGroupModels.AsEnumerable();

            return sheetModel;
        }
        #endregion
    }
}