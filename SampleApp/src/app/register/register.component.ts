import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/Alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder, ValidatorFn, AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  registerForm: FormGroup;


  constructor(private authService: AuthService, private alertify: AlertifyService, 
    private fb: FormBuilder) { }

  ngOnInit() {
    this.createRegisterForm();
  }

  createRegisterForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]]
    });
  }
  
  matchValues(matchTo: string): ValidatorFn{
    return (control: AbstractControl)=>{
      return control?.value === control?.parent?.controls[matchTo].value
      ? null : {mismatch: true}
    }
  }

  register(){
    this.authService.register(this.registerForm.getRawValue()).subscribe(() => {
      this.registerForm.reset();
      this.registerForm.controls['gender'].setValue('male');
      this.alertify.success('registration successful');
    }, error => {
      this.alertify.error(error);
    });
  }

  cancel(){
    this.cancelRegister.emit(false);
  }

}
