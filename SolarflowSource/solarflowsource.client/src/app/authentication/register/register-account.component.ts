import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../../services/authentication.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register-account',
  templateUrl: './register-account.component.html',
  styleUrls: ['./register-account.component.css'],
  imports: [FormsModule, CommonModule]
})
export class RegisterAccountComponent {
  name: string = "";
  email: string = "";
  password: string = "";
  confirmPassword: string = "";
  errors: { name?: string; email?: string; password?: string; confirmPassword?: string } = {};

  constructor(private authenticationService: AuthenticationService, private router: Router) { }

  register() {
    if (!this.validateInputs()) {
      return;
    }

    this.authenticationService.registerAccount(this.name, this.email, this.password).subscribe(
      (response) => {
        alert(response.message);
        this.router.navigate(['']);
      },
      (error) => {
        alert("Failed to register account: " + error.error);
      }
    );
  }

  validateInputs(): boolean {
    this.errors = {};
    let isValid = true;

    // Validate the name.
    if (this.name.length < 3 || /^\s/.test(this.name)) {
      this.errors.name = "Name must have atleast 3 characters and must not start with white space.";
      isValid = false;
    }

    // Validate the email.
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    if (!emailPattern.test(this.email)) {
      this.errors.email = "Invalid email format.";
      isValid = false;
    }

    // Validate the password.
    const passwordPattern = /^(?!.*\s).{8,}$/;
    if (!passwordPattern.test(this.password)) {
      this.errors.password = "Password must have atleast 8 characters with no white spaces.";
      isValid = false;
    }

    if (this.password !== this.confirmPassword) {
      this.errors.confirmPassword = "Passwords do not match.";
      isValid = false;
    }

    return isValid;
  }
}
