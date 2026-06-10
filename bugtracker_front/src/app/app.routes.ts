import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Projects } from './pages/projects/projects';
import { Bugs } from './pages/bugs/bugs';
import { Landing } from './pages/landing/landing';

export const routes: Routes = [

    {path: '', component: Landing},
    {path: 'login', component: Login},
    {path: 'register', component: Register},
    {path: 'projects', component: Projects},
    {path: 'bugs', component: Bugs},
];
