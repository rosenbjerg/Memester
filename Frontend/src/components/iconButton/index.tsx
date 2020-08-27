import {h} from "preact";
import * as style from "./style.css";
import { useCallback, useState } from "preact/hooks";

interface Props{
    icon: string;
    style?: string|object;
    class?: string;
    onClick: () => Promise<any>;
}

export default function IconButton(props:Props){
    const [disabled,setDisabled] = useState(false);
    const handleClick = useCallback(async ()=>{
        setDisabled(true);
        await props.onClick();
        setDisabled(false);
    }, [props.onClick])
    return(
        <button
            disabled={disabled}
            class={`${style.buttonStyle} material-icons ${props.class || ""}`}
            style={`${props.style||''}`}
            onClick={handleClick}>{props.icon}</button>

    )
}