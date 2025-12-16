import configOptions, {init} from "@github/markdownlint-github"
import rules from "./.markdownlint.json" with { type: "json" }

const options = {
    config: init({...rules}),
    customRules: ["@github/markdownlint-github"],
    outputFormatters: [
      [ "markdownlint-cli2-formatter-pretty", { "appendLink": true } ] // ensures the error message includes a link to the rule documentation
    ]
}
export default options