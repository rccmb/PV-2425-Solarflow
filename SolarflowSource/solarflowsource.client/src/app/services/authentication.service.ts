import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  private apiUrl = 'https://localhost:7212/api/user/';

  constructor(private http: HttpClient) { }

  authenticate(email: string, password: string): Observable<any> {
    return this.http.post(this.apiUrl + "authenticate", { email, password });
  }

  getUserProfile(): Observable<any> {
    const token = localStorage.getItem('authToken');
    const headers = new HttpHeaders({ 'Authorization': `Bearer ${token}` });
    return this.http.get(this.apiUrl + "profile", { headers });
  }

  recoverAccount(email: string): Observable<any> {
    return this.http.post(this.apiUrl + "recover-account", { email });
  }

  registerAccount(name: string, email: string, password: string): Observable<any> {
    return this.http.post(this.apiUrl + "register", {name, email, password})
  }
}
