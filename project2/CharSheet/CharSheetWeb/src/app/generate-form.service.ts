import { Injectable } from '@angular/core';
import * as $ from 'jquery';
import 'jqueryui';

import { FormTemplate } from './shared/form-template';

@Injectable({
  providedIn: 'root'
})
export class GenerateFormService {

  constructor() { }

  public getNewFormTemplate(): FormTemplate {
    let formTemplate = { title: ""} as FormTemplate;
    return formTemplate;
  }
}
