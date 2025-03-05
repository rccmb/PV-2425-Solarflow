import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { LoginComponent } from './authentication/login/login.component';
import { RecoverAccountComponent } from './authentication/recover-account/recover-account.component';
import { RegisterAccountComponent } from './authentication/register/register-account.component';

const routes: Routes = [
  { path: '', component: LoginComponent },
  { path: 'login/recover-account', component: RecoverAccountComponent },
  { path: 'login/register-account', component: RegisterAccountComponent },
  { path: 'dashboard', component: DashboardComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
