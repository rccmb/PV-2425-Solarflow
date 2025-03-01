import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../services/authentication.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; 

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  imports: [FormsModule, CommonModule]
})
export class DashboardComponent {
  userProfile: any;

  constructor(private router: Router, private authenticationService: AuthenticationService) { }

  testGetUserProfile() {
    this.authenticationService.getUserProfile().subscribe(
      (response) => {
        console.log('User Profile:', response);
        this.userProfile = response;
      },
      (error) => {
        console.error('Error fetching profile:', error);
      }
    );
  }
}
