import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../services/authentication.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  imports: [FormsModule]
})
export class LoginComponent {
  username: string = "";
  password: string = "";

  constructor(private authenticationService: AuthenticationService, private router: Router) { }

  login() {
    this.authenticationService.authenticate(this.username, this.password).subscribe(
      (response) => {
        console.log('Authentication OK', response);
        this.router.navigate(['/dashboard']);
      },
      (error) => {
        console.error('Authentication FAILED', error);
      }
    )
  }

  recoverAccount() {
    this.router.navigate(['login/recover-account']);
  }
}
