using System;
using System.CommandLine;

static void GetAllFiles(List<string> allFiles, IEnumerable<string> folders, List<string> unused)
{
    if (folders.Count() == 0)
        return;
    foreach (var folder in folders)
    {
        var files = Directory.EnumerateFiles(folder);//קבצים בתיקייה הנוכחית
        var names = files.Select(x => Path.GetFileName(x)).Except(unused);//שמות קבצים תקינים
        allFiles.AddRange(files.Where(x => names.Contains(Path.GetFileName(x))));

        var subFolders = Directory.EnumerateDirectories(folder);//תיקיות בתיקייה הנוכחית
        var sub = subFolders.Where(x =>
        subFolders.Select(y => Path.GetFileName(y)).Except(unused)
        .Contains(Path.GetFileName(x)));
        GetAllFiles(allFiles, sub, unused);
    }
}

List<string> unusedFiles = new List<string>() { ".angular", ".vscode", ".git", ".vs", "bin", "obj", "node_modules", "publish", ".json" };
List<string> languages = new List<string>() { "ts", "cs", "html", "js", "scss", "css", "c", "cpp", "java", "py" };





var rootCommand = new RootCommand("Root Command for Cli Bundle");
var bundleCommand = new Command("bundle", "Bundle code files to a single file");
bundleCommand.AddAlias("b");
//output command
var outputOption = new Option<FileInfo>("--output", "file path and name");
outputOption.AddAlias("-o");
outputOption.AddValidator((x) =>
{

   
        if(File.Exists(x.GetValueOrDefault().ToString()))
    {
        Console.WriteLine("a file with this name is already exist, do you want to overwrite it? Y/N");
        if(Console.ReadLine().ToUpper()=="Y")
        { Console.WriteLine("please enter the new name for the output file");
          //צריך לקלוט שם חדש מהמשתמש
        }
      
       
    }
        if(x.IsImplicit)
    {
        Console.WriteLine("the file name is going to be :output.txt ");
        Console.WriteLine("");
    }

        
            
        

    
    
    
});
//language option

var languageOption = new Option<string[]>("--languages", "languges extensions to bundle");
languageOption.AllowMultipleArgumentsPerToken = true;
languageOption.IsRequired = true;
languageOption.AddAlias("-l");
languageOption.AddValidator((langs) =>
{ foreach (var lang in langs.Children)
    {//check if the user entered wrong languages
        if(!languages.Contains(lang.ToString().ToLower())&&lang.ToString().ToLower()!="all")
        {
            Console.WriteLine($"{lang.ToString()} is not available language");

        }
    }

});
languageOption.SetDefaultValue("all");



var noteOption = new Option<bool>("--note", "write the source of the files");
noteOption.AddAlias("-n");
noteOption.SetDefaultValue(true);

bundleCommand.AddOption(noteOption);
var sortOption = new Option<bool>("--sort", "how to sort the files in the bundle file. The default is alphabetical ");
sortOption.AddAlias("-s");
sortOption.SetDefaultValue(true);
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines");
removeEmptyLinesOption.AddAlias("-r");
removeEmptyLinesOption.SetDefaultValue(true);

var autherOption = new Option<string>("--author", "write the author of this file");
autherOption.AddAlias("-a");
autherOption.AddValidator(s =>
{
    if (s.ToString() == "")
        Console.WriteLine("you didnt antered the auther name");

});
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(autherOption);
bundleCommand.AddOption(removeEmptyLinesOption);
rootCommand.AddCommand(bundleCommand);
rootCommand.InvokeAsync(args);

bundleCommand.SetHandler<FileInfo, string[],bool,bool,string,bool>((output,languages,note,sort,auther,remove ) =>
{     
   
    if (languages[0]=="all")
    {   
        languages = languages.ToArray();


    }

    var directory=Directory.GetCurrentDirectory();
     List<string> filesToCopy= new List<string>();//all the files to bundle
    var files = Directory.EnumerateFiles(directory);//all the files that exist in the currennt directory
    // 
    filesToCopy.AddRange(files.Where(x =>
    files.Select(x => Path.GetFileName(x)).Except(unusedFiles)
    .Contains(Path.GetFileName(x))));
    //
    var folders = Directory.EnumerateDirectories(directory);
    folders = folders.Where(x => folders.Select(x => Path.GetFileName(x)).Except(unusedFiles).Contains(Path.GetFileName(x))
    ).ToList();
    GetAllFiles(filesToCopy, folders, unusedFiles);//organize all the files to copy including the sub files
     if(sort)
        filesToCopy.OrderBy(x=>Path.GetExtension(x).ToList());
      else
        filesToCopy=filesToCopy.OrderBy(x=>Path.GetFileName(x)).ToList();

    try
    {
        using (var writer = new StreamWriter(output.FullName))
        {
            if (auther is not null)
            {
                writer.WriteLine("#" + auther);
                writer.WriteLine(); ;
            }
            foreach(var file in filesToCopy)
            {

                if (languages.Any(l => "." + l == Path.GetExtension(file)))
                {
                    using (var reader = new StreamReader(file))
                    {
                        string line;
                        bool isEmpty = true;
                        while ((line = reader.ReadLine()) != null)
                            if (!(remove && line == ""))
                            {
                                writer.WriteLine(line);
                                isEmpty = false;
                            }

                        writer.WriteLine();
                        if (note && !isEmpty)
                        {
                            writer.WriteLine("//code source (relative routing) /** " + Path.GetRelativePath(directory, file) + " **///");
                            writer.WriteLine();
                        }
                        writer.WriteLine();
                    }
                }
            } 


        }
    }

    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("ERORR: the directory is invalid");
        Console.WriteLine(ex.Message);
    }
    catch(IOException ex)
    {
        Console.WriteLine("erorr accured");
    }
    
},outputOption,languageOption,noteOption,sortOption,autherOption,removeEmptyLinesOption);



// RSP command
var rspCommand = new Command("create-rsp", " create a response file");