﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Our.Umbraco.Mortar.Web.Controllers
{
	[PluginController("MortarApi")]
	public class MortarApiController : UmbracoAuthorizedJsonController
	{
		[HttpGet]
		public IEnumerable<object> GetContentTypes([ModelBinder] string[] allowedContentTypes)
		{
			return Services.ContentTypeService.GetAllContentTypes()
				.Where(x => allowedContentTypes == null || allowedContentTypes.Length == 0 || allowedContentTypes.Any(y => Regex.IsMatch(x.Alias, y)))
				.OrderBy(x => x.SortOrder)
				.Select(x => new
				{
					id = x.Id,
					guid = x.Key,
					name = x.Name,
					alias = x.Alias,
					icon = x.Icon
				});
		}

		[HttpGet]
		public object GetDataTypePreValues(string dtdId)
		{
			Guid guidDtdId;
			int intDtdId;

			IDataTypeDefinition dtd;

			// Parse the ID
			if (int.TryParse(dtdId, out intDtdId))
			{
				// Do nothing, we just want the int ID
				dtd = Services.DataTypeService.GetDataTypeDefinitionById(intDtdId);
			}
			else if (Guid.TryParse(dtdId, out guidDtdId))
			{
				dtd = Services.DataTypeService.GetDataTypeDefinitionById(guidDtdId);
			}
			else
			{
				return null;
			}

			if (dtd == null)
				return null;

			// Convert to editor config
			var preValue = Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dtd.Id);
			var propEditor = PropertyEditorResolver.Current.GetByAlias(dtd.PropertyEditorAlias);
			return propEditor.PreValueEditor.ConvertDbToEditor(propEditor.DefaultPreValues, preValue);
		}
	}
}