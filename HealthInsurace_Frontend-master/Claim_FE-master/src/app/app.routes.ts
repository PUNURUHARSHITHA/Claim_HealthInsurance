import { Routes } from '@angular/router';
import { AdminLayout } from './admin/admin-layout';
import { PolicyholderLayout } from './policyholder/policyholder-layout';
import { ManagerLayoutComponent } from './manager/manager-layout';

export const routes: Routes = [
  // 🌐 Public routes
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./components/login/login').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./components/register/register').then(m => m. RegisterComponent) },
  { path: 'home', loadComponent: () => import('./components/home/home').then(m => m.Home) },
  
 {path:'logout',loadComponent:() =>import('./components/logout/logout').then(m=>m.LogoutComponent)},
  // 🛡️ Admin layout with child routes
  {
    path: 'admin',
    component: AdminLayout,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
       {path:'admin-dashboard',loadComponent:() =>import('./admin/admin-dashboard/admin-dashboard').then(m=>m.AdminDashboardComponent)},
      { path: 'policyholder-list', loadComponent: () => import('./admin/policyholder-list/policyholder-list').then(m => m.PolicyholderList) },
      { path: 'policy-types', loadComponent: () => import('./admin/policy-types/policy-types').then(m => m.PolicyTypes) },
      { path: 'treatments', loadComponent: () => import('./admin/treatments/treatments').then(m => m.Treatments) },
      { path: 'hospitals', loadComponent: () => import('./admin/hospitals/hospitals').then(m => m.Hospitals) },
      { path: 'agents', loadComponent: () => import('./admin/agents/agents').then(m => m.Agents) },
      { path: 'eligibility-check', loadComponent: () => import('./admin/eligibility-check/eligibility-check').then(m => m.EligibilityCheck) },
      { path: 'reports', loadComponent: () => import('./admin/reports/reports').then(m => m.Reports) },
      { path: 'claim-tracking', loadComponent: () => import('./admin/claim-tracking/claim-tracking').then(m => m.ClaimTracking) },
      { path: 'claim-underreview', loadComponent: () => import('./admin/claim-underreview/claim-underreview').then(m => m.ClaimUnderReviewComponent) },
      { path: 'claim-status', loadComponent: () => import('./admin/claim-status/claim-status').then(m => m.ClaimStatus) },
      {path:'payment-processes',loadComponent:() =>import('./admin/payment-processes/payment-processes').then(m => m.PaymentProcessesComponent)},
      
      
    ]
  },

  // 🛡️ Policyholder layout with child routes
  {
    path: 'policyholder',
    component: PolicyholderLayout,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
     
      { path: 'policyholder-dashboard', loadComponent: () => import('./policyholder/policyholder-dashboard/policyholder-dashboard').then(m => m.PolicyholderDashboardComponent) },
      { path: 'policy-types', loadComponent: () => import('./policyholder/policy-types/policy-types').then(m => m.PolicyTypeComponent) },
      { path: 'policyholder-form', loadComponent: () => import('./policyholder/policyholder-form/policyholder-form').then(m => m.PolicyholderComponent) },
      { path: 'treatments', loadComponent: () => import('./policyholder/treatments/treatments').then(m => m.TreatmentComponent) },
      { path: 'hospitals', loadComponent: () => import('./policyholder/hospitals/hospitals').then(m => m.HospitalComponent) },
      { path: 'agents', loadComponent: () => import('./policyholder/agents/agents').then(m => m.AgentComponent) },
      { path: 'claims', loadComponent: () => import('./policyholder/claims/claims').then(m => m.ClaimsComponent) },
      { path: 'claim-tracking', loadComponent: () => import('./policyholder/claim-tracking/claim-tracking').then(m => m.ClaimTracking) },
    ]
  },
 
// 🧭 Manager layout with child routes
  {
    path: 'manager',
    component: ManagerLayoutComponent,
    children: [
      { path: '', redirectTo: 'manager-panel', pathMatch: 'full' },
       {path:'dashboard',loadComponent:() =>import('./manager/manager-dashboard/manager-dashboard').then(m=>m.ManagerDashboardComponent)},
      { path: 'approval-1', loadComponent: () => import('./manager/approval-1/approval-1').then(m => m.Approval1) }
    ]
  },
  
  // 🧭 Wildcard fallback
  { path: '**', redirectTo: 'home' }
];
 