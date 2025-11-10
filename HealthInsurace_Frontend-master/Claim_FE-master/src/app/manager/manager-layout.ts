import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../components/navbar/navbar'; // ✅ Adjust path as needed
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-manager-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, NavbarComponent], // ✅ Import required components
  templateUrl: './manager-layout.html',
  styleUrls: ['./manager-layout.css']
})
export class ManagerLayoutComponent {}