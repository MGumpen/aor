            {
                logger.LogError(ex, "Database migrate/seed failed at startup. Application will continue to run.");
            }
            else
            {
                Console.Error.WriteLine("Database migrate/seed failed: " + ex);
            }
        }
    }

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.AddServerHeader = false;
    });
    // Configure pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
