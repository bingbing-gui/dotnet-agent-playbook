```mermaId
graph LR;
  %% Bottom Layer: Database and Storage
  A[IdentityDbContext] -->|Stores Data| B[IUserStore<TUser>]
  A -->|Stores Data| C[IRoleStore<TRole>]

  %% Storage Layer
  B -->|Implementation| D[UserStore]
  C -->|Implementation| E[RoleStore]

  %% User Management Layer
  D -->|Depends On| F[IUserManager<TUser>]
  E -->|Depends On| G[IRoleManager<TRole>]
  
  F -->|Implementation| H[UserManager]
  G -->|Implementation| I[RoleManager]

  %% Authentication Management Layer
  H -->|Depends On| J[ISignInManager<TUser>]
  H -->|Manages User Claims| K[ClaimsPrincipal]
  I -->|Manages Role Claims| K

  J -->|Implementation| L[SignInManager]
  L -->|Depends On| M[IAuthenticationService]
  
  %% Authentication Methods Layer (Top Layer)
  M --> N1[Cookies Authentication]
  M --> N2[JWT Authentication]
  M --> N3[OAuth Authentication]
  M --> N4[OpenID Connect Authentication]

  %% Detailed Methods (Core Only)
  H --> H1[CreateAsync Create User]
  H --> H2[DeleteAsync Delete User]
  H --> H3[UpdateAsync Update User]
  H --> H4[FindByIdAsync Find User]
  H --> H5[CheckPasswordAsync Check Password]
  H --> H6[GeneratePasswordResetTokenAsync Generate Password Reset Token]

  I --> I1[CreateAsync Create Role]
  I --> I2[DeleteAsync Delete Role]
  I --> I3[UpdateAsync Update Role]
  I --> I4[FindByIdAsync Find Role]

  L --> L1[PasswordSignInAsync Password Sign-In]
  L --> L2[SignInAsync Manual Sign-In]
  L --> L3[SignOutAsync Sign Out]
  L --> L4[TwoFactorSignInAsync Two-Factor Sign-In]
  L --> L5[ExternalLoginSignInAsync External Login Sign-In]

```
