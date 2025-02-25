import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent {
  username: string = "";
  password: string = "";

  constructor(private authService: AuthService) { }

  login() {
    this.authService.authenticate(this.username, this.password).subscribe(
      (response) => {
        console.log('Authentication OK', response);
      },
      (error) => {
        console.error('Authentication FAILED', error);
      }
    )
  }
}
