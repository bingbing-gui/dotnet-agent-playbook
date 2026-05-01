```mermaId
graph LR;
  %% 底层：数据库与存储
  A[IdentityDbContext] -->|存储数据| B[IUserStore<TUser>]
  A -->|存储数据| C[IRoleStore<TRole>]

  %% 存储层
  B -->|实现| D[UserStore]
  C -->|实现| E[RoleStore]

  %% 用户管理层
  D -->|依赖| F[IUserManager<TUser>]
  E -->|依赖| G[IRoleManager<TRole>]
  
  F -->|实现| H[UserManager]
  G -->|实现| I[RoleManager]

  %% 认证管理层
  H -->|依赖| J[ISignInManager<TUser>]
  H -->|管理用户声明| K[ClaimsPrincipal]
  I -->|管理角色声明| K

  J -->|实现| L[SignInManager]
  L -->|依赖| M[IAuthenticationService]
  
  %% 认证方式层（最上层）
  M --> N1[Cookies 认证]
  M --> N2[JWT 认证]
  M --> N3[OAuth 认证]
  M --> N4[OpenID Connect 认证]

  %% 详细方法（仅列出核心）
  H --> H1[CreateAsync 创建用户]
  H --> H2[DeleteAsync 删除用户]
  H --> H3[UpdateAsync 更新用户]
  H --> H4[FindByIdAsync 查找用户]
  H --> H5[CheckPasswordAsync 检查密码]
  H --> H6[GeneratePasswordResetTokenAsync 生成密码重置令牌]

  I --> I1[CreateAsync 创建角色]
  I --> I2[DeleteAsync 删除角色]
  I --> I3[UpdateAsync 更新角色]
  I --> I4[FindByIdAsync 查找角色]

  L --> L1[PasswordSignInAsync 密码登录]
  L --> L2[SignInAsync 手动登录]
  L --> L3[SignOutAsync 注销用户]
  L --> L4[TwoFactorSignInAsync 双因素登录]
  L --> L5[ExternalLoginSignInAsync 外部登录]

```