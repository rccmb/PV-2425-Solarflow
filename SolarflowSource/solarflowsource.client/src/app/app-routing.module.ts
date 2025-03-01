import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { LoginComponent } from './login/login.component';
import { RecoverAccountComponent } from './login/recover-account/recover-account.component';

const routes: Routes = [
  { path: '', component: LoginComponent },
  { path: 'login/recover-account', component: RecoverAccountComponent },
  { path: 'dashboard', component: DashboardComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
